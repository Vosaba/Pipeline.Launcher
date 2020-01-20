using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Abstractions.PipelineStage;
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
        public virtual StageBaseConfiguration Configuration { get; }

        protected abstract int[] GetItemsHashCode(TInput input, PipelineStageContext context);

        protected abstract object[] GetOriginalItems(TInput input);

        protected abstract TOutput GetExceptionItem(TInput input, Exception ex, PipelineStageContext context);

        protected abstract Task<TOutput> InternalExecute(TInput input, PipelineStageContext context);

        public async Task<TOutput> BaseExecute(TInput input, PipelineStageContext context, int tryCount = 0)
        {
            Func<int[]> getItemsHashCode = null;
            if (context.ActionsSet?.DiagnosticHandler != null)
            {
                int[] itemsHashCode = null;
                getItemsHashCode = () =>
                {
                    if (itemsHashCode == null)
                    {
                        itemsHashCode = GetItemsHashCode(input, context);
                    }

                    return itemsHashCode;
                };

                context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(getItemsHashCode, GetType(), DiagnosticState.Enter));
            }

            try
            {
                var result = await InternalExecute(input, context);

                context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(getItemsHashCode, GetType(), DiagnosticState.Process));

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
                        if (tryCount >= context.ExecutionTryCount)
                        {
                            return GetExceptionItem(input, new StageRetryCountException(context.ExecutionTryCount), context);
                        }

                        return await BaseExecute(input, context, ++tryCount);
                    }
                }

                context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(getItemsHashCode, GetType(), DiagnosticState.ExceptionOccured, ex.Message));

                return GetExceptionItem(input, ex, context);
            }
        }
    }
}