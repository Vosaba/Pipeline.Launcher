using System;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using PipelineLauncher.Exceptions;

namespace PipelineLauncher.PipelineStage
{
    public interface IPipelineBaseStage<in TInput, TOutput>
    {
        Task<TOutput> BaseExecute(TInput input, PipelineStageContext context, int tryCount = 0);
    }

    public abstract class PipelineBaseStage<TInput, TOutput> : IPipelineBaseStage<TInput, TOutput>
    {
        public abstract StageBaseConfiguration BaseConfiguration { get; }

        protected abstract object[] GetOriginalItems(TInput input);
        protected abstract object[] GetOriginalItems(TOutput output);
        protected abstract TOutput GetExceptionItem(TInput input, Exception ex, PipelineStageContext context);

        protected abstract Task<TOutput> InternalExecute(TInput input, PipelineStageContext context);

        public async Task<TOutput> BaseExecute(TInput input, PipelineStageContext context, int tryCount = 0)
        {
            try
            {
                context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(GetOriginalItems(input), GetType(), DiagnosticState.Input));
                var result = await InternalExecute(input, context);

                context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(GetOriginalItems(result), GetType(), DiagnosticState.Output));
                return result;
            }
            catch (Exception ex)
            {
                if (context.ActionsSet?.ExceptionHandler != null)
                {
                    var shouldBeReExecuted = false;
                    void Retry() => shouldBeReExecuted = true;

                    context.ActionsSet?.ExceptionHandler(new ExceptionItemsEventArgs(GetOriginalItems(input), GetType(), ex, Retry));

                    if (shouldBeReExecuted)
                    {
                        if (tryCount >= BaseConfiguration.MaxRetriesCount)
                        {
                            var retryException = new StageRetryCountException(BaseConfiguration.MaxRetriesCount);

                            context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(GetOriginalItems(input), GetType(), DiagnosticState.ExceptionOccured, retryException.Message));
                            return GetExceptionItem(input, retryException, context);
                        }

                        context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(GetOriginalItems(input), GetType(), DiagnosticState.Retry, ex.Message));
                        return await BaseExecute(input, context, ++tryCount);
                    }
                }

                context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(GetOriginalItems(input), GetType(), DiagnosticState.ExceptionOccured, ex.Message));
                return GetExceptionItem(input, ex, context);
            }
        }
    }
}