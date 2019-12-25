using System;
using PipelineLauncher.Abstractions.PipelineEvents;

namespace PipelineLauncher.Abstractions.Dto
{
    public class ActionsSet
    {
        public Action ReExecute { get; }
        public Action<DiagnosticEventArgs> DiagnosticAction { get; }

        public ActionsSet(Action reExecute, Action<DiagnosticEventArgs> diagnosticAction)
        {
            ReExecute = reExecute;
            DiagnosticAction = diagnosticAction;
        }
    }
}