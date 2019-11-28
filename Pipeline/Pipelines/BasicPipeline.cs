using PipelineLauncher.Abstractions.Dto;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Attributes;

namespace PipelineLauncher.Pipelines
{
    internal class BasicPipeline<TInput, TOutput> : IPipeline<TInput, TOutput>
    {
        protected readonly CancellationToken _cancellationToken;

        protected readonly ITargetBlock<PipelineItem<TInput>> _firstBlock;
        protected readonly ISourceBlock<PipelineItem<TOutput>> _lastBlock;
        protected readonly ActionBlock<PipelineItem<TOutput>> _sortingBlock;

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

        internal BasicPipeline(
            ITargetBlock<PipelineItem<TInput>> firstBlock,
            ISourceBlock<PipelineItem<TOutput>> lastBlock,
            CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;

            _firstBlock = firstBlock;
            _lastBlock = lastBlock;


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

    internal class AwaitablePipeline<TInput, TOutput> : BasicPipeline<TInput, TOutput>,  IAwaitablePipeline<TInput, TOutput>
    {
        private readonly ConcurrentBag<TOutput> _processedItems = new ConcurrentBag<TOutput>();

        internal AwaitablePipeline(
            ITargetBlock<PipelineItem<TInput>> firstBlock,
            ISourceBlock<PipelineItem<TOutput>> lastBlock,
            CancellationToken cancellationToken)
            : base(firstBlock, lastBlock, cancellationToken)
        {

            ItemReceivedEvent += AwaitablePipeline_ItemReceivedEvent;

            _lastBlock.Completion.ContinueWith(x =>
            {
                _sortingBlock.Complete();
            });

            _sortingBlock.Completion.ContinueWith(x =>
            {
                //_processedItems.CompleteAdding();
            });
        }

        private void AwaitablePipeline_ItemReceivedEvent(TOutput item)
        {
            _processedItems.Add(item);
        }

        public Task AwaitableTask()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TOutput> RunSync(TInput input)
        {
            return RunSync(new []{input});
        }

        public IEnumerable<TOutput> RunSync(IEnumerable<TInput> input)
        {
            base.Post(input);
            _processedItems.Clear();

            _firstBlock.Complete();
            _lastBlock.Completion.Wait();
            //_processedItems.CompleteAdding();

            return _processedItems;
        }
    }
}
