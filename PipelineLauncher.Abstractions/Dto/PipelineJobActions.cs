using System;
using PipelineLauncher.Abstractions.PipelineEvents;

namespace PipelineLauncher.Abstractions.Dto
{
    public class ActionsSet
    {
        public Action ReExecute { get; private set; }
        public Action<DiagnosticItem> DiagnosticAction { get; private set; }

        public ActionsSet SetDiagnosticAction(Action<DiagnosticItem> diagnosticAction)
        {
            DiagnosticAction = diagnosticAction;
            return this;
        }

        public ActionsSet SetReExecuteAction(Action reExecute)
        {
            ReExecute = reExecute;
            return this;
        }
    }

    //public enum DiagnosticActionsLog
}