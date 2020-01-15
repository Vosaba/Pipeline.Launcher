using System;
using System.Linq;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineEvents;

namespace PipelineLauncher.Abstractions.PipelineStage.Dto
{
    public class ActionsSet
    {
        public ActionsSet(Action reExecute, Action<ExceptionItemsEventArgs> exceptionHandler, Action<DiagnosticItem> diagnosticHandler, Func<object[], int[]> getItemsIdentify)
        {
            ReExecute = reExecute;
            ExceptionHandler = exceptionHandler;
            DiagnosticHandler = diagnosticHandler;

            if (getItemsIdentify != null)
            {
                GetItemsHashCode = items => getItemsIdentify(items.Where(x => x != null).ToArray());
            }
            else
            {
                GetItemsHashCode = items =>
                {
                    return items.Where(x => x != null).Select(x => x.GetHashCode()).ToArray();
                };
            }
        }

        public Action ReExecute { get; }
        public Action<ExceptionItemsEventArgs> ExceptionHandler { get; }
        public Action<DiagnosticItem> DiagnosticHandler { get; }
        public Func<object[], int[]> GetItemsHashCode { get; }
    }
}