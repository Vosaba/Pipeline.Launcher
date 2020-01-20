using System;
using System.Linq;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineEvents;

namespace PipelineLauncher.Abstractions.PipelineStage.Dto
{
    public class ActionsSet
    {
        public ActionsSet(Action retry, Action<ExceptionItemsEventArgs> exceptionHandler, Action<DiagnosticItem> diagnosticHandler, Func<object, int> getItemIdentify)
        {
            Retry = retry;
            ExceptionHandler = exceptionHandler;
            DiagnosticHandler = diagnosticHandler;

            if (getItemIdentify != null)
            {
                GetItemsHashCode = getItemIdentify;
            }
            else
            {
                GetItemsHashCode = item => item?.GetHashCode() ?? 0;
            }
        }

        public Action Retry { get; }
        public Action<ExceptionItemsEventArgs> ExceptionHandler { get; }
        public Action<DiagnosticItem> DiagnosticHandler { get; }
        public Func<object, int> GetItemsHashCode { get; }
    }
}