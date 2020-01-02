using System;
using PipelineLauncher.Abstractions.PipelineEvents;

namespace PipelineLauncher.Abstractions.PipelineStage.Dto
{
    public class ActionsSet
    {
        public ActionsSet(Action reExecute, Action<DiagnosticItem> diagnosticAction)
        {
            ReExecute = reExecute;
            DiagnosticAction = diagnosticAction;
        }

        public Action ReExecute { get; }
        public Action<DiagnosticItem> DiagnosticAction { get; }
    }
}