using System;

namespace PipelineLauncher.Abstractions.PipelineEvents
{
    public class PipelineEventArgs
    {
        public Type StageType { get; }
        public string StageName => StageType.FullName;

        public PipelineEventArgs(Type stageType)
        {
            StageType = stageType;
        }
    }

    public class ExceptionItemsEventArgs : PipelineEventArgs
    {
        public object[] Items { get; }
        public Exception Exception { get; }

        public Action ReProcess { get; }

        public ExceptionItemsEventArgs(object[] items, Type stageType, Exception exception, Action reProcess)
            : base(stageType)
        {
            Items = items;
            ReProcess = reProcess;
            Exception = exception;
        }
    }

    public class SkippedItemEventArgs : PipelineEventArgs
    {
        public object Item { get; }

        public SkippedItemEventArgs(object item, Type stageType)
            : base(stageType)
        {
            Item = item;
        }
    }

    public struct DiagnosticEventArgs
    {
        public Type StageType { get; }
        public TimeSpan StartTime { get; } 
        public TimeSpan FinishTime { get; private set; }

        public DiagnosticState State { get; private set; }
        public string Message { get; private set; }

        public TimeSpan RunningTime => StartTime - FinishTime;

        public DiagnosticEventArgs(Type stageType)
        {
            StageType = stageType;
            StartTime = DateTime.Now.TimeOfDay;
            FinishTime = StartTime;
            State = DiagnosticState.Started;
            Message = null;
        }

        public DiagnosticEventArgs Finish(DiagnosticState diagnosticState = DiagnosticState.Finished, string message = null)
        {
            FinishTime = DateTime.Now.TimeOfDay;
            State = diagnosticState;
            Message = message;

            return this;
        }
    }


    public enum DiagnosticState
    {
        Started,
        Finished,
        ExceptionOccured,
        Skipped
    }
}