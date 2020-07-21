using System;
using System.Linq;

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

    public class SkippedItemEventArgs : PipelineEventArgs
    {
        public object Item { get; }

        public SkippedItemEventArgs(object item, Type stageType)
            : base(stageType)
        {
            Item = item;
        }
    }

    public class SkippedItemEventArgs<TItem> : SkippedItemEventArgs
    {
        public new TItem Item => (TItem)base.Item;

        public SkippedItemEventArgs(object item, Type stageType)
            : base(item, stageType)
        {
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

    public class ExceptionItemsEventArgs<TItem> : ExceptionItemsEventArgs
    {
        public new TItem[] Items => base.Items.Select(x => (TItem) x).ToArray();

        public ExceptionItemsEventArgs(object[] items, Type stageType, Exception exception, Action retry)
            : base(items, stageType, exception, retry)
        {
        }
    }
}