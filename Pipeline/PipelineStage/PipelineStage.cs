using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using PipelineLauncher.Exceptions;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Stages;

namespace PipelineLauncher.PipelineStage
{
    internal class PipelineStage<TInput, TOutput> : PipelineBaseStage<PipelineStageItem<TInput>, PipelineStageItem<TOutput>>
    {
        private readonly IStage<TInput, TOutput> _stage;

        public PipelineStage(IStage<TInput, TOutput> stage)
        {
            _stage = stage;
        }

        public StageConfiguration Configuration => _stage.Configuration;

        protected override StageBaseConfiguration BaseConfiguration => Configuration;

        protected override Type StageType => _stage.GetType();

        protected Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken) =>
            _stage.ExecuteAsync(input, cancellationToken);

        protected override async Task<PipelineStageItem<TOutput>> InternalExecute(PipelineStageItem<TInput> input, StageExecutionContext executionContext)
        {
            try
            {
                switch (input)
                {
                    case ExceptionStageItem<TInput> exceptionItem:
                        return exceptionItem.Return<TOutput>();

                    case NonResultStageItem<TInput> noneItem when noneItem.ReadyToProcess<TInput>(StageType):
                        return new PipelineStageItem<TOutput>(await ExecuteAsync((TInput)noneItem.OriginalItem, executionContext.CancellationToken));

                    case NonResultStageItem<TInput> nonItem:
                        executionContext.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(new[] { nonItem.OriginalItem }, StageType, DiagnosticState.Skip));
                        return nonItem.Return<TOutput>();

                    default:
                        return new PipelineStageItem<TOutput>(await ExecuteAsync(input.Item, executionContext.CancellationToken));
                }
            }
            catch (NonResultStageItemException<TOutput> e)
            {
                executionContext.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(new [] { e.StageItem.OriginalItem } , StageType, DiagnosticState.Skip));
                return e.StageItem;
            }
        }

        protected override PipelineStageItem<TOutput> GetExceptionItem(PipelineStageItem<TInput> input, Exception ex, StageExecutionContext executionContext)
        {
            return new ExceptionStageItem<TOutput>(ex, executionContext.ActionsSet?.Retry, GetType(), GetItemsToBeProcessed(input));
        }

        protected override object[] GetItemsToBeProcessed(PipelineStageItem<TInput> input)
        {
            switch (input)
            {
                case NonResultStageItem<TInput> noneItem when noneItem.ReadyToProcess<TInput>(StageType):
                    return new object[] { noneItem.OriginalItem };

                case NonResultStageItem<TInput> noneItem:
                    return new object[] { };

                default:
                    return new object[] { input.Item };
            }
        }

        protected override object[] GetProcessedItems(PipelineStageItem<TOutput> output)
        {
            switch (output)
            {
                case NonResultStageItem<TOutput> noneResultItem:
                    return new object[] { }; 
                default:
                    return new object[] { output.Item };
            }
        }
    }
}
