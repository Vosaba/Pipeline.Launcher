using System;
using System.Linq;

namespace PipelineLauncher.Abstractions.Dto
{
    public struct DiagnosticItem
    {
        public int[] ItemsHashCode { get; }

        public Type StageType { get; }
        public TimeSpan TimeSpan { get; } 
        public DiagnosticState State { get; }
        public string Message { get; }

        public string StageName => StageType.FullName;
        
        public DiagnosticItem(Func<int[]> getItemsHashCode, Type stageType, DiagnosticState diagnosticState, string message = null)
        {
            StageType = stageType;
            TimeSpan = DateTime.Now.TimeOfDay;
            State = diagnosticState;
            Message = message;
            ItemsHashCode = getItemsHashCode();
        }
    }

    public enum DiagnosticState
    {
        Enter,
        Process,
        ExceptionOccured,
        Skip
    }
}