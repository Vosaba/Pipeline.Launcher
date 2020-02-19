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
        Task<TOutput> BaseExecute(TInput input, StageExecutionContext executionContext, int tryCount = 0);
    }

    internal abstract class PipelineBaseStage<TInput, TOutput> : IPipelineBaseStage<TInput, TOutput>
    {
        protected abstract StageBaseConfiguration BaseConfiguration { get; }

        protected abstract Type StageType { get; }

        protected abstract object[] GetOriginalItems(TInput input);
        protected abstract object[] GetOriginalItems(TOutput output);
        protected abstract TOutput GetExceptionItem(TInput input, Exception ex, StageExecutionContext executionContext);

        protected abstract Task<TOutput> InternalExecute(TInput input, StageExecutionContext executionContext);

        public async Task<TOutput> BaseExecute(TInput input, StageExecutionContext executionContext, int tryCount = 0)
        {
            try
            {
                if (executionContext.ActionsSet?.DiagnosticHandler != null)
                {
                    var itemsToLog = GetOriginalItems(input);
                    if (itemsToLog.Any())
                    {
                        executionContext.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(itemsToLog, GetType(), DiagnosticState.Input));
                    }
                }

                var result = await InternalExecute(input, executionContext);

                if (executionContext.ActionsSet?.DiagnosticHandler != null)
                {
                    var itemsToLog = GetOriginalItems(result);
                    if (itemsToLog.Any())
                    {
                        executionContext.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(itemsToLog, GetType(), DiagnosticState.Output));
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                object[] itemsToLog = null;

                if (executionContext.ActionsSet?.DiagnosticHandler != null)
                {
                    itemsToLog = GetOriginalItems(input);
                }

                if (executionContext.ActionsSet?.ExceptionHandler != null)
                {
                    var shouldBeReExecuted = false;
                    void Retry() => shouldBeReExecuted = true;

                    executionContext.ActionsSet?.ExceptionHandler(new ExceptionItemsEventArgs(itemsToLog, GetType(), ex, Retry));

                    if (shouldBeReExecuted)
                    {
                        if (tryCount >= BaseConfiguration.MaxRetriesCount)
                        {
                            var retryException = new StageRetryCountException(BaseConfiguration.MaxRetriesCount);

                            executionContext.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(itemsToLog, GetType(), DiagnosticState.ExceptionOccured, retryException.Message));
                            return GetExceptionItem(input, retryException, executionContext);
                        }

                        executionContext.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(itemsToLog, GetType(), DiagnosticState.Retry, ex.Message));
                        return await BaseExecute(input, executionContext, ++tryCount);
                    }
                }

                executionContext.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(itemsToLog, GetType(), DiagnosticState.ExceptionOccured, ex.Message));
                return GetExceptionItem(input, ex, executionContext);
            }
        }
    }
}