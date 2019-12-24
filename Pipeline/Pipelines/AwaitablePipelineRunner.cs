using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Dto;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.PipelineEvents;

namespace PipelineLauncher.Pipelines
{
    internal class AwaitablePipelineRunner<TInput, TOutput> : PipelineRunner<TInput, TOutput>, IAwaitablePipelineRunner<TInput, TOutput>
    {
        private readonly AwaitablePipelineConfig _pipelineConfig;
        private readonly ConcurrentBag<TOutput> _processedItems = new ConcurrentBag<TOutput>();

        private readonly Func<ITargetBlock<PipelineItem<TInput>>> _getFirstBlock;
        private readonly Func<ISourceBlock<PipelineItem<TOutput>>> _getLastBlock;
        private readonly Action _destroyTaskStages;

        internal AwaitablePipelineRunner(
            Func<ITargetBlock<PipelineItem<TInput>>> firstBlock,
            Func<ISourceBlock<PipelineItem<TOutput>>> lastBlock,
            CancellationToken cancellationToken,
            Action destroyTaskStages,
            AwaitablePipelineConfig pipelineConfig)
            : base(null, null, cancellationToken, false)
        {
            _pipelineConfig = pipelineConfig;
            _getFirstBlock = firstBlock;
            _getLastBlock = lastBlock;
            _destroyTaskStages = destroyTaskStages;

            ItemReceivedEvent += AwaitablePipeline_ItemReceivedEvent;
            ExceptionItemsReceivedEvent += AwaitablePipelineRunner_ExceptionItemsReceivedEvent;
        }

        public IEnumerable<TOutput> Process(TInput input)
        {
            return Process(new []{input});
        }

        public IEnumerable<TOutput> Process(IEnumerable<TInput> input)
        {
            InitBlocks();
            _processedItems.Clear();

            Post(input);

            FirstBlock.Complete();
            SortingBlock.Completion.Wait();

            return _processedItems;
        }

        public IObservable<TOutput> ProcessAsObservable(TInput input)
        {
            return ProcessAsObservable(new [] {input});
        }

        public IObservable<TOutput> ProcessAsObservable(IEnumerable<TInput> input)
        {
            InitBlocks();
            _processedItems.Clear();

            Post(input);
            FirstBlock.Complete();
            throw new Exception();
            //return ResultConsumerBlock.AsObservable();
        }

        //public async IAsyncEnumerable<TOutput> Process(IEnumerable<TInput> input, bool f)
        //{
        //    InitBlocks();
        //    _processedItems.Clear();

        //    Post(input);
        //    FirstBlock.Complete();

        //    await ResultConsumerBlock.Completion;
        //    foreach (var item in _processedItems)
        //    {
        //        yield return item;
        //    }
        //}

        private void AwaitablePipeline_ItemReceivedEvent(TOutput item)
        {
            _processedItems.Add(item);
        }

        private void AwaitablePipelineRunner_ExceptionItemsReceivedEvent(ExceptionItemsEventArgs items)
        {
            if (_pipelineConfig != null && _pipelineConfig.ThrowExceptionOccured)
            {
                throw items.Exception;
            }
        }

        private void InitBlocks()
        {
            _destroyTaskStages();
            FirstBlock = _getFirstBlock();
            LastBlock = _getLastBlock();

            InitSortingBlock();

            LastBlock.Completion.ContinueWith(x =>
            {
                SortingBlock.Complete();
                //ExceptionConsumerBlock.Complete();
                //SkippedConsumerBlock.Complete();
            });
        }
    }
}