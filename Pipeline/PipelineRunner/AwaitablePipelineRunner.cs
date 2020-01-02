using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineEvents;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Configurations;
using PipelineLauncher.Abstractions.PipelineStage;

namespace PipelineLauncher.PipelineRunner
{
    internal class AwaitablePipelineRunner<TInput, TOutput> : PipelineRunner<TInput, TOutput>, IAwaitablePipelineRunner<TInput, TOutput>
    {
        protected override StageCreationOptions CreationOptions => new StageCreationOptions(PipelineType.Awaitable);

        private readonly AwaitablePipelineConfig _pipelineConfig;
        private readonly ConcurrentBag<TOutput> _processedItems = new ConcurrentBag<TOutput>();

        private readonly Action _destroyTaskStages;

        internal AwaitablePipelineRunner(
            Func<StageCreationOptions, bool, ITargetBlock<PipelineStageItem<TInput>>> retrieveFirstBlock,
            Func<StageCreationOptions, bool, ISourceBlock<PipelineStageItem<TOutput>>> retrieveLastBlock,
            CancellationToken cancellationToken,
            Action destroyTaskStages,
            AwaitablePipelineConfig pipelineConfig)
            : base(retrieveFirstBlock, retrieveLastBlock, cancellationToken, false)
        {
            _pipelineConfig = pipelineConfig;
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

            RetrieveFirstBlock(CreationOptions, true).Complete();
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
            //RetrieveFirstBlock.Complete();
            throw new Exception();
            //return ResultConsumerBlock.AsObservable();
        }

        //public async IAsyncEnumerable<TOutput> Process(IEnumerable<TInput> input, bool f)
        //{
        //    InitBlocks();
        //    _processedItems.Clear();

        //    Post(input);
        //    RetrieveFirstBlock.Complete();

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

            InitSortingBlock();

            RetrieveLastBlock(CreationOptions, true).Completion.ContinueWith(x =>
            {
                SortingBlock.Complete();
                //ExceptionConsumerBlock.Complete();
                //SkippedConsumerBlock.Complete();
            });
        }
    }
}