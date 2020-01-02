using System;
using PipelineLauncher.Abstractions.PipelineEvents;

namespace PipelineLauncher.Abstractions.Dto
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



        //public ActionsSet SetDiagnosticAction(Action<DiagnosticItem> diagnosticAction)
        //{
        //    DiagnosticAction = diagnosticAction;
        //    return this;
        //}

        //public ActionsSet SetReExecuteAction(Action reExecute)
        //{
        //    ReExecute = reExecute;
        //    return this;
        //}
    }
}