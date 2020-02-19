using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using PipelineLauncher.Exceptions;

namespace PipelineLauncher.PipelineStage
{
    internal interface IPipelineBaseStage<in TInput, TOutput>
    {
        Task<TOutput> BaseExecute(TInput input, PipelineStageContext context, int tryCount = 0);
    }

    internal abstract class PipelineBaseStage<TInput, TOutput> : IPipelineBaseStage<TInput, TOutput>
    {
        protected abstract StageBaseConfiguration BaseConfiguration { get; }

        protected abstract Type StageType { get; }

        protected abstract object[] GetOriginalItems(TInput input);
        protected abstract object[] GetOriginalItems(TOutput output);
        protected abstract TOutput GetExceptionItem(TInput input, Exception ex, PipelineStageContext context);

        protected abstract Task<TOutput> InternalExecute(TInput input, PipelineStageContext context);

        public async Task<TOutput> BaseExecute(TInput input, PipelineStageContext context, int tryCount = 0)
        {
            try
            {
                if (context.ActionsSet?.DiagnosticHandler != null)
                {
                    var itemsToLog = GetOriginalItems(input);
                    if (itemsToLog.Any())
                    {
                        context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(itemsToLog, GetType(), DiagnosticState.Input));
                    }
                }

                var result = await InternalExecute(input, context);

                if (context.ActionsSet?.DiagnosticHandler != null)
                {
                    var itemsToLog = GetOriginalItems(result);
                    if (itemsToLog.Any())
                    {
                        context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(itemsToLog, GetType(), DiagnosticState.Output));
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                object[] itemsToLog = null;

                if (context.ActionsSet?.DiagnosticHandler != null)
                {
                    itemsToLog = GetOriginalItems(input);
                }

                if (context.ActionsSet?.ExceptionHandler != null)
                {
                    var shouldBeReExecuted = false;
                    void Retry() => shouldBeReExecuted = true;

                    context.ActionsSet?.ExceptionHandler(new ExceptionItemsEventArgs(itemsToLog, GetType(), ex, Retry));

                    if (shouldBeReExecuted)
                    {
                        if (tryCount >= BaseConfiguration.MaxRetriesCount)
                        {
                            var retryException = new StageRetryCountException(BaseConfiguration.MaxRetriesCount);

                            context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(itemsToLog, GetType(), DiagnosticState.ExceptionOccured, retryException.Message));
                            return GetExceptionItem(input, retryException, context);
                        }

                        context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(itemsToLog, GetType(), DiagnosticState.Retry, ex.Message));
                        return await BaseExecute(input, context, ++tryCount);
                    }
                }

                context.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(itemsToLog, GetType(), DiagnosticState.ExceptionOccured, ex.Message));
                return GetExceptionItem(input, ex, context);
            }
        }
    }
}