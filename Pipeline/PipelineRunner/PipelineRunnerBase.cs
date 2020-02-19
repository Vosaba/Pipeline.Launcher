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
        protected abstract StageCreationOptions CreationOptions { get; }

        protected readonly PipelineSetupContext PipelineSetupContext;

        protected Func<StageCreationOptions, bool, ITargetBlock<PipelineStageItem<TInput>>> RetrieveFirstBlock;
        protected Func<StageCreationOptions, bool, ISourceBlock<PipelineStageItem<TOutput>>> RetrieveLastBlock;
        protected Func<ISourceBlock<PipelineStageItem<TOutput>>, ActionBlock<PipelineStageItem<TOutput>>> GenerateSortingBlock;

        public event ItemReceivedEventHandler<TOutput> ItemReceivedEvent;
        public event ExceptionItemsReceivedEventHandler ExceptionItemsReceivedEvent;
        public event SkippedItemReceivedEventHandler SkippedItemReceivedEvent;
        public event DiagnosticEventHandler DiagnosticEvent
        {
            add => PipelineSetupContext.DiagnosticEvent += value;
            remove => PipelineSetupContext.DiagnosticEvent -= value;
        }

        internal PipelineRunnerBase(
            Func<StageCreationOptions, bool, ITargetBlock<PipelineStageItem<TInput>>> retrieveFirstBlock,
            Func<StageCreationOptions, bool, ISourceBlock<PipelineStageItem<TOutput>>> retrieveLastBlock,
            PipelineSetupContext pipelineSetupContext)
        {
            PipelineSetupContext = pipelineSetupContext;

            RetrieveFirstBlock = retrieveFirstBlock;
            RetrieveLastBlock = retrieveLastBlock;

            GenerateSortingBlock = block =>
            {
                var sortingBlock = new ActionBlock<PipelineStageItem<TOutput>>(SortingMethod);

                block.LinkTo(sortingBlock, new DataflowLinkOptions { PropagateCompletion = false });

                block.Completion.ContinueWith(e =>
                {
                    sortingBlock.Complete();
                });//, PipelineSetupContext.CancellationToken);

                return sortingBlock;
            };
        }

        public IPipelineRunnerBase<TInput, TOutput> SetupCancellationToken(CancellationToken cancellationToken)
        {
            PipelineSetupContext.SetupCancellationToken(cancellationToken);
            return this;
        }

        protected void SortingMethod(PipelineStageItem<TOutput> input)
        {
            switch (input)
            {
                case ExceptionStageItem<TOutput> exceptionItem:
                    ExceptionItemsReceivedEvent?.Invoke(new ExceptionItemsEventArgs(exceptionItem.FailedItems, exceptionItem.StageType, exceptionItem.Exception, exceptionItem.Retry));
                    return;
                case NoneResultStageItem<TOutput> nonResultItem:
                    SkippedItemReceivedEvent?.Invoke(new SkippedItemEventArgs(nonResultItem.OriginalItem, nonResultItem.StageType));
                    return;
                default:
                    ItemReceivedEvent?.Invoke(input.Item);
                    return;
            }
        }
    }
}