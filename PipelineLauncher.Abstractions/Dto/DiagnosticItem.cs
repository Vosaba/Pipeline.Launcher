using System;

namespace PipelineLauncher.Abstractions.Dto
{
    public struct DiagnosticItem
    {
        public object Input { get; }

        public Type StageType { get; }
        public TimeSpan TimeSpan { get; } 
        public DiagnosticState State { get; }
        public string Message { get; }

        public string StageName => StageType.FullName;

        public DiagnosticItem(object input, Type stageType, DiagnosticState diagnosticState, string message = null)
        {
            StageType = stageType;
            TimeSpan = DateTime.Now.TimeOfDay;
            State = diagnosticState;
            Message = message;
            Input = input;
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