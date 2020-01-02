using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.PipelineStage;

namespace PipelineLauncher.PipelineRunner
{
    internal abstract class PipelineRunnerBase<TInput, TOutput>
    {
        protected abstract StageCreationOptions CreationOptions { get; }

        protected readonly CancellationToken CancellationToken;

        protected Func<StageCreationOptions, bool, ITargetBlock<PipelineStageItem<TInput>>> RetrieveFirstBlock;
        protected Func<StageCreationOptions, bool, ISourceBlock<PipelineStageItem<TOutput>>> RetrieveLastBlock;
        protected Func<ISourceBlock<PipelineStageItem<TOutput>>, ActionBlock<PipelineStageItem<TOutput>>> GenerateSortingBlock;

        public event ItemReceivedEventHandler<TOutput> ItemReceivedEvent;
        public event ExceptionItemsReceivedEventHandler ExceptionItemsReceivedEvent;
        public event SkippedItemReceivedEventHandler SkippedItemReceivedEvent;

        internal PipelineRunnerBase(
            Func<StageCreationOptions, bool, ITargetBlock<PipelineStageItem<TInput>>> retrieveFirstBlock,
            Func<StageCreationOptions, bool, ISourceBlock<PipelineStageItem<TOutput>>> retrieveLastBlock,
            CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;

            RetrieveFirstBlock = retrieveFirstBlock;
            RetrieveLastBlock = retrieveLastBlock;

            GenerateSortingBlock = block =>
            {
                var sortingBlock = new ActionBlock<PipelineStageItem<TOutput>>(input =>
                {
                    switch (input)
                    {
                        case ExceptionStageItem<TOutput> exceptionItem:
                            ExceptionItemsReceivedEvent?.Invoke(new ExceptionItemsEventArgs(exceptionItem.FailedItems, exceptionItem.StageType, exceptionItem.Exception, exceptionItem.ReProcessItems));
                            return;
                        case NoneResultStageItem<TOutput> nonResultItem:
                            SkippedItemReceivedEvent?.Invoke(new SkippedItemEventArgs(nonResultItem.OriginalItem, nonResultItem.StageType));
                            return;
                        default:
                            ItemReceivedEvent?.Invoke(input.Item);
                            return;
                    }
                });

                block.LinkTo(sortingBlock, new DataflowLinkOptions { PropagateCompletion = false });

                return sortingBlock;
            };
        }
    }
}