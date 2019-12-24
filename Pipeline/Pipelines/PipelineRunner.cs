using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.PipelineEvents;

namespace PipelineLauncher.Pipelines
{
    internal class PipelineRunner<TInput, TOutput> : IPipelineRunner<TInput, TOutput>
    {
        protected readonly CancellationToken CancellationToken;

        protected ITargetBlock<PipelineItem<TInput>> FirstBlock;
        protected ISourceBlock<PipelineItem<TOutput>> LastBlock;
        protected ActionBlock<PipelineItem<TOutput>> SortingBlock;

        public event ItemReceivedEventHandler<TOutput> ItemReceivedEvent;
        public event ExceptionItemsReceivedEventHandler ExceptionItemsReceivedEvent;
        public event SkippedItemReceivedEventHandler SkippedItemReceivedEvent;

        public bool Post(TInput input)
        {
            return Post(new [] {input});
        }

        public bool Post(IEnumerable<TInput> input)
        {
            return input.Select(x => new PipelineItem<TInput>(x)).All(x => FirstBlock.Post(x));
        }

        internal PipelineRunner(
            ITargetBlock<PipelineItem<TInput>> firstBlock,
            ISourceBlock<PipelineItem<TOutput>> lastBlock,
            CancellationToken cancellationToken,
            bool initSortingBlock = true)
        {
            CancellationToken = cancellationToken;

            FirstBlock = firstBlock;
            LastBlock = lastBlock;

            if (initSortingBlock)
            {
                InitSortingBlock();
            }
        }

        protected void InitSortingBlock()
        {
            SortingBlock = new ActionBlock<PipelineItem<TOutput>>(input =>
            {
                switch (input)
                {
                    case ExceptionItem<TOutput> exceptionItem:
                        ExceptionItemsReceivedEvent?.Invoke(new ExceptionItemsEventArgs(exceptionItem.FailedItems, exceptionItem.StageType, exceptionItem.Exception, exceptionItem.ReProcessItems));
                        return;
                    case NoneResultItem<TOutput> nonResultItem:
                        SkippedItemReceivedEvent?.Invoke(new SkippedItemEventArgs(nonResultItem.OriginalItem, nonResultItem.StageType));
                        return;
                    default:
                        ItemReceivedEvent?.Invoke(input.Item);
                        return;
                }
            });

            LastBlock.LinkTo(SortingBlock, new DataflowLinkOptions { PropagateCompletion = false });
        }
    }
}
