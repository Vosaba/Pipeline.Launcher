using System;
using System.Linq;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineEvents;

namespace PipelineLauncher.Abstractions.PipelineStage.Dto
{
    public class ActionsSet
    {
        public ActionsSet(Action retry, Action<ExceptionItemsEventArgs> instantExceptionHandler, DiagnosticEventHandler diagnosticHandler)
        {
            Retry = retry;
            InstantExceptionHandler = instantExceptionHandler;
            DiagnosticHandler = diagnosticHandler;
        }

        public Action Retry { get; }
        public Action<ExceptionItemsEventArgs> InstantExceptionHandler { get; }
        public DiagnosticEventHandler DiagnosticHandler { get; }
    }
}