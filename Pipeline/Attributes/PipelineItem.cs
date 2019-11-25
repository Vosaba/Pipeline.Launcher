using System;
using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.Attributes
{
    internal abstract class NonResultItem<TItem> : PipelineItem<TItem>
    {
        protected NonResultItem() : base(default) { }

        public abstract NonResultItem<TNewItem> Return<TNewItem>();
    }

    internal class RemoveItem<TItem> : NonResultItem<TItem>
    {
        public object OriginalItem { get; }

        public RemoveItem(object originalItem)
        {
            OriginalItem = originalItem;
        }

        public override NonResultItem<TNewItem> Return<TNewItem>()
        {
            return new RemoveItem<TNewItem>(OriginalItem);
        }
    }

    internal class SkipItem<TItem> : NonResultItem<TItem>
    {
        public object OriginalItem { get; }

        public SkipItem(object originalItem)
        {
            OriginalItem = originalItem;
        }

        public override NonResultItem<TNewItem> Return<TNewItem>()
        {
            return new SkipItem<TNewItem>(OriginalItem);
        }
    }

    internal class SkipItemTill<TItem> : NonResultItem<TItem>
    {
        public object OriginalItem { get; }
        public Type JobType { get; }

        public SkipItemTill(Type jobType, object item)
        {
            JobType = jobType;
            OriginalItem = item;
        }

        public override NonResultItem<TNewItem> Return<TNewItem>()
        {
            return new SkipItemTill<TNewItem>(JobType, OriginalItem);
        }
    }

    internal class ExceptionItem<TItem> : NonResultItem<TItem>
    {
        public object[] FailedItems { get; }
        public Exception Exception { get; }

        public ExceptionItem(Exception exception, params object[] failedItems)
        {
            FailedItems = failedItems;
            Exception = exception;
        }

        public override NonResultItem<TNewItem> Return<TNewItem>()
        {
            return new ExceptionItem<TNewItem>(Exception, FailedItems);
        }
    }
}