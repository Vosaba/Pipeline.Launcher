using PipelineLauncher.Abstractions.PipelineStage;
using System;

namespace PipelineLauncher.PipelineStage
{
    internal class PipelineStageItem
    {
        public object Item { get; }

        private PipelineStageItem() { }

        public PipelineStageItem(object item)
        {
            Item = item;
        }
    }

    internal class PipelineStageItem<TItem> : PipelineStageItem
    {
        public new TItem Item { get; }

        public PipelineStageItem(TItem item) : base(item)
        {
            Item = item;
        }
    }

    internal abstract class NonResultStageItem<TItem> : PipelineStageItem<TItem>
    {
        public object OriginalItem { get; }
        public Type OriginalItemType => OriginalItem.GetType();
        public Type StageType { get; }

        protected NonResultStageItem(object originalItem, Type stageType) : base(default)
        {
            StageType = stageType;
            OriginalItem = originalItem;

        }

        public abstract NonResultStageItem<TNewItem> Return<TNewItem>();

        public abstract bool ReadyToProcess<TInput>(Type stageType);
    }

    internal class RemoveStageItem<TItem> : NonResultStageItem<TItem>
    {
        public RemoveStageItem(object originalItem, Type stageType): base(originalItem, stageType)
        {
        }

        public override NonResultStageItem<TNewItem> Return<TNewItem>()
        {
            return new RemoveStageItem<TNewItem>(OriginalItem, StageType);
        }

        public override bool ReadyToProcess<TInput>(Type stageType)
        {
            return false;
        }
    }

    internal class SkipStageItem<TItem> : NonResultStageItem<TItem>
    {
        private bool _readyToProcess { get; }

        public SkipStageItem(object originalItem, Type stageType, bool readyToProcess = false) : base(originalItem, stageType)
        {
            _readyToProcess = readyToProcess;
        }

        public override NonResultStageItem<TNewItem> Return<TNewItem>()
        {
            return new SkipStageItem<TNewItem>(OriginalItem, StageType, true);
        }

        public override bool ReadyToProcess<TInput>(Type stageType)
        {
            return _readyToProcess && typeof(TInput) == OriginalItemType;
        }
    }

    internal class SkipStageItemTill<TItem> : NonResultStageItem<TItem>
    {
        public Type SkipTillType { get; }

        public SkipStageItemTill(Type skipTillType, object originalItem, Type stageType) : base(originalItem, stageType)
        {
            SkipTillType = skipTillType;
        }

        public override NonResultStageItem<TNewItem> Return<TNewItem>()
        {
            return new SkipStageItemTill<TNewItem>(SkipTillType, OriginalItem, StageType);
        }

        public override bool ReadyToProcess<TInput>(Type stageType)
        {
            return SkipTillType == stageType && OriginalItemType == typeof(TInput);
        }
    }

    internal class ExceptionStageItem<TItem> : NonResultStageItem<TItem>
    {
        public object[] FailedItems => (object[])OriginalItem;
        public Exception Exception { get; }
        public Action Retry { get; }

        public ExceptionStageItem(Exception exception, Action reProcess, Type stageType, params object[] failedItems)
            : base(failedItems, stageType)
        {
            Exception = exception;
            Retry = reProcess;
        }

        public override NonResultStageItem<TNewItem> Return<TNewItem>()
        {
            return new ExceptionStageItem<TNewItem>(Exception, Retry, StageType, FailedItems);
        }

        public override bool ReadyToProcess<TInput>(Type stageType)
        {
            return false;
        }
    }
}