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

        public Action Retry { get; }

        public ExceptionItemsEventArgs(object[] items, Type stageType, Exception exception, Action retry)
            : base(stageType)
        {
            Items = items;
            Retry = retry;
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
}