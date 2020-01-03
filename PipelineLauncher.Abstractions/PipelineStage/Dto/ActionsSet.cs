using System;
using System.Linq;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineEvents;

namespace PipelineLauncher.Abstractions.PipelineStage.Dto
{
    public class ActionsSet
    {
        public ActionsSet(Action reExecute, Action<DiagnosticItem> diagnosticAction, Func<object[], int[]> getItemsIdentify)
        {
            ReExecute = reExecute;
            DiagnosticAction = diagnosticAction;

            if (getItemsIdentify != null)
            {
                GetItemsHashCode = (items) => getItemsIdentify(items.Where(e => e!= null).ToArray());
            }
            else
            {
                GetItemsHashCode = items =>
                {
                    return items.Where(e => e != null).Select(e => e.GetHashCode()).ToArray();
                };
            }
        }

        public Action ReExecute { get; }
        public Action<DiagnosticItem> DiagnosticAction { get; }
        public Func<object[], int[]> GetItemsHashCode { get; }
    }
}