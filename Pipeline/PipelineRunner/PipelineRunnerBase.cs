using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Abstractions.PipelineRunner;
using PipelineLauncher.PipelineStage;

namespace PipelineLauncher.PipelineRunner
{
    internal abstract class PipelineRunnerBase<TInput, TOutput> : IPipelineRunnerBase<TInput, TOutput>
    {
        private readonly Dictionary<Type, TypedHandler> _eventTypeHandlers = new Dictionary<Type, TypedHandler>();

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

        public ITypedHandler<T> TypedHandler<T>()
        {
            if (_eventTypeHandlers.ContainsKey(typeof(T)))
            {
                return (ITypedHandler<T>)_eventTypeHandlers[typeof(T)];
            }

            var typeHandler = new TypedHandler<T>();
            _eventTypeHandlers.Add(typeof(T), typeHandler);
            return typeHandler;
        }

        public IPipelineRunnerBase<TInput, TOutput> WithModifier(Action<IPipelineRunnerBase<TInput, TOutput>> modifier)
        {
            modifier(this);
            return this;
        }

        protected void SortingMethod(PipelineStageItem<TOutput> input)
        {
            switch (input)
            {
                case ExceptionStageItem<TOutput> exceptionItem:
                {
                    var exceptionItemsEventArgs = new ExceptionItemsEventArgs(exceptionItem.FailedItems, exceptionItem.StageType, exceptionItem.Exception, exceptionItem.Retry);

                    if (_eventTypeHandlers.ContainsKey(exceptionItem.OriginalItemType) && _eventTypeHandlers[exceptionItem.OriginalItemType].TryToHandleExceptionItems(exceptionItemsEventArgs))
                    {
                        return;
                    }

                    ExceptionItemsReceivedEvent?.Invoke(exceptionItemsEventArgs);
                    return;
                }
                case NonResultStageItem<TOutput> nonResultItem:
                {
                    var skippedItemEventArgs = new SkippedItemEventArgs(nonResultItem.OriginalItem, nonResultItem.StageType);

                    if (_eventTypeHandlers.ContainsKey(nonResultItem.OriginalItemType) && _eventTypeHandlers[nonResultItem.OriginalItemType].TryToHandleSkippedItem(skippedItemEventArgs))
                    {
                        return;
                    }

                    SkippedItemReceivedEvent?.Invoke(skippedItemEventArgs);
                    return;
                }
                default:
                    ItemReceivedEvent?.Invoke(input.Item);
                    return;
            }
        }
    }
}