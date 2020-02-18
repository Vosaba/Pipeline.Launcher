using System;

namespace PipelineLauncher.Abstractions.Dto
{
    public struct DiagnosticItem
    {
        public object[] Items { get; }

        public Type StageType { get; }
        public TimeSpan TimeSpan { get; } 
        public DiagnosticState State { get; }
        public string Message { get; }

        public string StageName => StageType.FullName;

        public DiagnosticItem(object[] items, Type stageType, DiagnosticState diagnosticState, string message = null)
        {
            StageType = stageType;
            TimeSpan = DateTime.Now.TimeOfDay;
            State = diagnosticState;
            Message = message;
            Items = items;
        }
    }

    public enum DiagnosticState
    {
        Input,
        Output,
        ExceptionOccured,
        Retry,
        Skip
    }
}