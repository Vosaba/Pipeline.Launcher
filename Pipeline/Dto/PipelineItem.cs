using System;
using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.Dto
{
    internal abstract class NoneResultItem<TItem> : PipelineItem<TItem>
    {
        public object OriginalItem { get; }
        public Type StageType { get; }

        protected NoneResultItem(object originalItem, Type stageType) : base(default)
        {
            StageType = stageType;
            OriginalItem = originalItem;

        }

        public abstract NoneResultItem<TNewItem> Return<TNewItem>();
    }

    internal class RemoveItem<TItem> : NoneResultItem<TItem>
    {
        public RemoveItem(object originalItem, Type stageType): base(originalItem, stageType)
        {
        }

        public override NoneResultItem<TNewItem> Return<TNewItem>()
        {
            return new RemoveItem<TNewItem>(OriginalItem, StageType);
        }
    }

    internal class SkipItem<TItem> : NoneResultItem<TItem>
    {
        public SkipItem(object originalItem, Type stageType) : base(originalItem, stageType)
        {
        }

        public override NoneResultItem<TNewItem> Return<TNewItem>()
        {
            return new SkipItem<TNewItem>(OriginalItem, StageType);
        }
    }

    internal class SkipItemTill<TItem> : NoneResultItem<TItem>
    {
        public Type SkipTillType { get; }

        public SkipItemTill(Type skipTillType, object originalItem, Type stageType) : base(originalItem, stageType)
        {
            SkipTillType = skipTillType;
        }

        public override NoneResultItem<TNewItem> Return<TNewItem>()
        {
            return new SkipItemTill<TNewItem>(SkipTillType, OriginalItem, StageType);
        }
    }

    internal class ExceptionItem<TItem> : NoneResultItem<TItem>
    {
        public object[] FailedItems => (object[])OriginalItem;
        public Exception Exception { get; }
        public Action ReProcessItems { get; }

        public ExceptionItem(Exception exception, Action reProcess, Type stageType, params object[] failedItems)
            : base(failedItems, stageType)
        {
            Exception = exception;
            ReProcessItems = reProcess;
        }

        public override NoneResultItem<TNewItem> Return<TNewItem>()
        {
            return new ExceptionItem<TNewItem>(Exception, ReProcessItems, StageType, FailedItems);
        }
    }
}