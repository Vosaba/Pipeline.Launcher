using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Abstractions.PipelineRunner;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.PipelineSetup;
using PipelineLauncher.PipelineStage;

namespace PipelineLauncher.PipelineRunner
{
    internal abstract class PipelineRunnerBase<TInput, TOutput> : IPipelineRunnerBase<TInput, TOutput>
    {
        public event ItemReceivedEventHandler<TOutput> ItemReceivedEvent;
        public event ExceptionItemsReceivedEventHandler ExceptionItemsReceivedEvent;
        public event SkippedItemReceivedEventHandler SkippedItemReceivedEvent;
        public event DiagnosticEventHandler DiagnosticEvent
        {
            add => StageCreationContext.DiagnosticEvent += value;
            remove => StageCreationContext.DiagnosticEvent -= value;
        }

        protected StageCreationContext StageCreationContext { get; }
        protected Func<StageCreationContext, ITargetBlock<PipelineStageItem<TInput>>> RetrieveFirstBlock { get; }
        protected Func<StageCreationContext, ISourceBlock<PipelineStageItem<TOutput>>> RetrieveLastBlock { get; }
        protected Func<ISourceBlock<PipelineStageItem<TOutput>>, ActionBlock<PipelineStageItem<TOutput>>> GenerateSortingBlock { get; }

        internal PipelineRunnerBase(
            Func<StageCreationContext, ITargetBlock<PipelineStageItem<TInput>>> retrieveFirstBlock,
            Func<StageCreationContext, ISourceBlock<PipelineStageItem<TOutput>>> retrieveLastBlock,
            StageCreationContext stageCreationContext)
        {
            RetrieveFirstBlock = retrieveFirstBlock;
            RetrieveLastBlock = retrieveLastBlock;
            StageCreationContext = stageCreationContext;

            GenerateSortingBlock = sourceBlock =>
            {
                var sortingBlock = new ActionBlock<PipelineStageItem<TOutput>>(SortingMethod);

                sourceBlock.LinkTo(sortingBlock, new DataflowLinkOptions { PropagateCompletion = false });

                sourceBlock.Completion.ContinueWith(e =>
                {
                    sortingBlock.Complete();
                });

                return sortingBlock;
            };
        }

        public IPipelineRunnerBase<TInput, TOutput> SetupCancellationToken(CancellationToken cancellationToken)
        {
            StageCreationContext.SetupCancellationToken(cancellationToken);
            return this;
        }

        protected void SortingMethod(PipelineStageItem<TOutput> input)
        {
            switch (input)
            {
                case ExceptionStageItem<TOutput> exceptionItem:
                    ExceptionItemsReceivedEvent?.Invoke(new ExceptionItemsEventArgs(exceptionItem.FailedItems, exceptionItem.StageType, exceptionItem.Exception, exceptionItem.Retry));
                    return;
                case NonResultStageItem<TOutput> nonResultItem:
                    SkippedItemReceivedEvent?.Invoke(new SkippedItemEventArgs(nonResultItem.OriginalItem, nonResultItem.StageType));
                    return;
                default:
                    ItemReceivedEvent?.Invoke(input.Item);
                    return;
            }
        }
    }
}