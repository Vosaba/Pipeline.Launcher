using System;
using System.Linq;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineEvents;

namespace PipelineLauncher.Abstractions.PipelineStage.Dto
{
    public class ActionsSet
    {
        public ActionsSet(Action retry, Action<ExceptionItemsEventArgs> exceptionHandler, Action<DiagnosticItem> diagnosticHandler)
        {
            Retry = retry;
            ExceptionHandler = exceptionHandler;
            DiagnosticHandler = diagnosticHandler;
        }

        public Action Retry { get; }
        public Action<ExceptionItemsEventArgs> ExceptionHandler { get; }
        public Action<DiagnosticItem> DiagnosticHandler { get; }
    }
}