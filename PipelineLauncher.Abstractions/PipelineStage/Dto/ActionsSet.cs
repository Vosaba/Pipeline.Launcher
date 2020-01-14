using System;
using System.Linq;
using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.Abstractions.PipelineStage.Dto
{
    public class ActionsSet
    {
        public ActionsSet(Action reExecute, Action<int> processed, Action<DiagnosticItem> diagnosticAction, Func<object[], int[]> getItemsIdentify)
        {
            ReExecute = reExecute;
            Processed = processed;
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
        public Action<int> Processed { get; }
        public Action<DiagnosticItem> DiagnosticAction { get; }
        public Func<object[], int[]> GetItemsHashCode { get; }
    }
}