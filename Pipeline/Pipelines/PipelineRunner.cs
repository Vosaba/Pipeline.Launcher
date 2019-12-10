using PipelineLauncher.Abstractions.Dto;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Attributes;
using PipelineLauncher.PipelineEvents;

namespace PipelineLauncher.Pipelines
{
    internal class PipelineRunner<TInput, TOutput> : IPipelineRunner<TInput, TOutput>
    {
        protected readonly CancellationToken _cancellationToken;

        protected ITargetBlock<PipelineItem<TInput>> _firstBlock;
        protected ISourceBlock<PipelineItem<TOutput>> _lastBlock;
        protected ActionBlock<PipelineItem<TOutput>> _sortingBlock;

        public event ItemReceivedEventHandler<TOutput> ItemReceivedEvent;
        public event ExceptionItemsReceivedEventHandler ExceptionItemsReceivedEvent;
        public event SkippedItemReceivedEventHandler SkippedItemReceivedEvent;

        public bool Post(TInput input)
        {
            return Post(new [] {input});
        }

        public bool Post(IEnumerable<TInput> input)
        {
            return input.Select(x => new PipelineItem<TInput>(x)).All(x => _firstBlock.Post(x));
        }

        internal PipelineRunner(
            ITargetBlock<PipelineItem<TInput>> firstBlock,
            ISourceBlock<PipelineItem<TOutput>> lastBlock,
            CancellationToken cancellationToken,
            bool initSortingBlock = true)
        {
            _cancellationToken = cancellationToken;

            _firstBlock = firstBlock;
            _lastBlock = lastBlock;

            if(initSortingBlock)
                InitSortingBlock();
        }

        protected void InitSortingBlock()
        {
            _sortingBlock = new ActionBlock<PipelineItem<TOutput>>(input =>
            {
                switch (input)
                {
                    case ExceptionItem<TOutput> exceptionItem:
                        ExceptionItemsReceivedEvent?.Invoke(new ExceptionItemsEventArgs(exceptionItem.FailedItems, exceptionItem.StageType, exceptionItem.Exception, exceptionItem.ReProcessItems));
                        return;
                    case NonResultItem<TOutput> nonResultItem:
                        SkippedItemReceivedEvent?.Invoke(new SkippedItemEventArgs(nonResultItem.OriginalItem, nonResultItem.StageType));
                        return;
                    default:
                        ItemReceivedEvent?.Invoke(input.Item);
                        return;
                }
            });

            _lastBlock.LinkTo(_sortingBlock, new DataflowLinkOptions { PropagateCompletion = false });
        }
    }

    internal class AwaitablePipelineRunner<TInput, TOutput> : PipelineRunner<TInput, TOutput>,  IAwaitablePipelineRunner<TInput, TOutput>
    {
        private readonly ConcurrentBag<TOutput> _processedItems = new ConcurrentBag<TOutput>();

        private readonly Func<ITargetBlock<PipelineItem<TInput>>> _getFirstBlock;
        private readonly Func<ISourceBlock<PipelineItem<TOutput>>> _getLastBlock;
        private readonly Action _destroyTaskStages;

        internal AwaitablePipelineRunner(
            Func<ITargetBlock<PipelineItem<TInput>>> firstBlock,
            Func<ISourceBlock<PipelineItem<TOutput>>> lastBlock,
            CancellationToken cancellationToken,
            Action destroyTaskStages)
            : base(null, null, cancellationToken, false)
        {
            _getFirstBlock = firstBlock;
            _getLastBlock = lastBlock;
            _destroyTaskStages = destroyTaskStages;

            ItemReceivedEvent += AwaitablePipeline_ItemReceivedEvent;
        }

        public IEnumerable<TOutput> Process(TInput input)
        {
            return Process(new []{input});
        }

        public IEnumerable<TOutput> Process(IEnumerable<TInput> input)
        {
            InitFirstLastSortingBlocks();
            _processedItems.Clear();

            Post(input);

            _firstBlock.Complete();
            _sortingBlock.Completion.Wait();

            return _processedItems;
        }

        public async IAsyncEnumerable<TOutput> Process(IEnumerable<TInput> input, bool f)
        {
           //ItemReceivedEvent += AwaitablePipelineRunner_ItemReceivedEvent; ;

            InitFirstLastSortingBlocks();
            _processedItems.Clear();

            Post(input);
            _firstBlock.Complete();


            //_sortingBlock.().OnNext(e);



            await _sortingBlock.Completion;
            foreach (var item in _processedItems)
            {
                yield return item;
            }
        }

        private void AwaitablePipeline_ItemReceivedEvent(TOutput item)
        {
            _processedItems.Add(item);
        }

        private void InitFirstLastSortingBlocks()
        {
            _destroyTaskStages();
            _firstBlock = _getFirstBlock();
            _lastBlock = _getLastBlock();

            InitSortingBlock();

            _lastBlock.Completion.ContinueWith(x =>
            {
                _sortingBlock.Complete();
            });
        }
    }
}
