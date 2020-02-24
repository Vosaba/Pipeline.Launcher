using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using PipelineLauncher.Exceptions;
using System;
using System.Linq;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.PipelineStage;

namespace PipelineLauncher.PipelineStage
{
    internal interface IPipelineBaseStage<TInput, TOutput> : IPipelineStage<TInput, TOutput>
    {
        Task<TOutput> BaseExecute(TInput input, StageExecutionContext executionContext, int tryCount = 0);
    }

    internal abstract class PipelineBaseStage<TInput, TOutput> : IPipelineBaseStage<TInput, TOutput>
    {
        protected abstract StageBaseConfiguration BaseConfiguration { get; }

        protected abstract Type StageType { get; }

        protected abstract object[] GetItemsToBeProcessed(TInput input);
        protected abstract object[] GetProcessedItems(TOutput output);
        protected abstract TOutput GetExceptionItem(TInput input, Exception ex, StageExecutionContext executionContext);

        protected abstract Task<TOutput> InternalExecute(TInput input, StageExecutionContext executionContext);

        public async Task<TOutput> BaseExecute(TInput input, StageExecutionContext executionContext, int tryCount = 0)
        {
            try
            {
                if (executionContext.ActionsSet?.DiagnosticHandler != null)
                {
                    var itemsToLog = GetItemsToBeProcessed(input);
                    if (itemsToLog.Any())
                    {
                        executionContext.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(itemsToLog, StageType, DiagnosticState.Input));
                    }
                }

                var output = await InternalExecute(input, executionContext);

                if (executionContext.ActionsSet?.DiagnosticHandler != null)
                {
                    var itemsToLog = GetProcessedItems(output);
                    if (itemsToLog.Any())
                    {
                        executionContext.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(itemsToLog, StageType, DiagnosticState.Output));
                    }
                }

                return output;
            }
            catch (Exception ex)
            {
                object[] exceptionItems  = GetItemsToBeProcessed(input);

                if (executionContext.ActionsSet?.InstantExceptionHandler != null)
                {
                    var shouldBeReExecuted = false;
                    void Retry() => shouldBeReExecuted = true;

                    executionContext.ActionsSet?.InstantExceptionHandler(new ExceptionItemsEventArgs(exceptionItems, StageType, ex, Retry));

                    if (shouldBeReExecuted)
                    {
                        if (tryCount >= BaseConfiguration.MaxRetriesCount)
                        {
                            var retryException = new StageRetryCountException(BaseConfiguration.MaxRetriesCount);

                            executionContext.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(exceptionItems, StageType, DiagnosticState.ExceptionOccured, retryException.Message));
                            return GetExceptionItem(input, retryException, executionContext);
                        }

                        executionContext.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(exceptionItems, StageType, DiagnosticState.Retry, ex.Message));
                        return await BaseExecute(input, executionContext, ++tryCount);
                    }
                }

                executionContext.ActionsSet?.DiagnosticHandler?.Invoke(new DiagnosticItem(exceptionItems, StageType, DiagnosticState.ExceptionOccured, ex.Message));
                return GetExceptionItem(input, ex, executionContext);
            }
        }
    }
}