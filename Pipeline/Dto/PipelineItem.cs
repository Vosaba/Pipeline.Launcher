using System;
using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.Dto
{
    internal abstract class NonResultItem<TItem> : PipelineItem<TItem>
    {
        public object OriginalItem { get; }
        public Type StageType { get; }

        protected NonResultItem(object originalItem, Type stageType) : base(default)
        {
            StageType = stageType;
            OriginalItem = originalItem;

        }

        public abstract NonResultItem<TNewItem> Return<TNewItem>();
    }

    internal class RemoveItem<TItem> : NonResultItem<TItem>
    {
        public RemoveItem(object originalItem, Type stageType): base(originalItem, stageType)
        {
        }

        public override NonResultItem<TNewItem> Return<TNewItem>()
        {
            return new RemoveItem<TNewItem>(OriginalItem, StageType);
        }
    }

    internal class SkipItem<TItem> : NonResultItem<TItem>
    {
        public SkipItem(object originalItem, Type stageType) : base(originalItem, stageType)
        {
        }

        public override NonResultItem<TNewItem> Return<TNewItem>()
        {
            return new SkipItem<TNewItem>(OriginalItem, StageType);
        }
    }

    internal class SkipItemTill<TItem> : NonResultItem<TItem>
    {
        public Type SkipTillType { get; }

        public SkipItemTill(Type skipTillType, object originalItem, Type stageType) : base(originalItem, stageType)
        {
            SkipTillType = skipTillType;
        }

        public override NonResultItem<TNewItem> Return<TNewItem>()
        {
            return new SkipItemTill<TNewItem>(SkipTillType, OriginalItem, StageType);
        }
    }

    internal class ExceptionItem<TItem> : NonResultItem<TItem>
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

        public override NonResultItem<TNewItem> Return<TNewItem>()
        {
            return new ExceptionItem<TNewItem>(Exception, ReProcessItems, StageType, FailedItems);
        }
    }
}