using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using PipelineLauncher.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PipelineLauncher.PipelineStage
{
    internal class PipelineStage<TInput, TOutput> : PipelineBaseStage<PipelineStageItem<TInput>, PipelineStageItem<TOutput>>
    {
        private readonly IPipelineStage<TInput, TOutput> _pipelineStage;

        public PipelineStage(IPipelineStage<TInput, TOutput> pipelineStage)
        {
            _pipelineStage = pipelineStage;
        }

        public StageConfiguration Configuration => _pipelineStage.Configuration;

        protected override StageBaseConfiguration BaseConfiguration => Configuration;

        protected override Type StageType => _pipelineStage.GetType();

        protected Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken) =>
            _pipelineStage.ExecuteAsync(input, cancellationToken);

        protected override async Task<PipelineStageItem<TOutput>> InternalExecute(PipelineStageItem<TInput> input, PipelineStageContext context)
        {
            try
            {
                switch (input)
                {
                    case ExceptionStageItem<TInput> exceptionItem:
                        return exceptionItem.Return<TOutput>();
                    case NonResultStageItem<TInput> noneItem when noneItem.ReadyToProcess<TInput>(StageType):
                        return new PipelineStageItem<TOutput>(await ExecuteAsync((TInput)noneItem.OriginalItem, context.CancellationToken));

                    case NonResultStageItem<TInput> nonItem:
                        context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(new[] { nonItem.OriginalItem }, GetType(), DiagnosticState.Skip));
                        return nonItem.Return<TOutput>();

                    default:
                        return new PipelineStageItem<TOutput>(await ExecuteAsync(input.Item, context.CancellationToken));
                }
            }
            catch (NoneParamException<TOutput> e)
            {
                context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(new [] {e.StageItem.OriginalItem} , GetType(), DiagnosticState.Skip));
                return e.StageItem;
            }
        }

        protected override PipelineStageItem<TOutput> GetExceptionItem(PipelineStageItem<TInput> input, Exception ex, PipelineStageContext context)
        {
            return new ExceptionStageItem<TOutput>(ex, context.ActionsSet?.Retry, GetType(), GetOriginalItems(input));
        }

        protected override object[] GetOriginalItems(PipelineStageItem<TInput> input)
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

        protected override object[] GetOriginalItems(PipelineStageItem<TOutput> output)
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
