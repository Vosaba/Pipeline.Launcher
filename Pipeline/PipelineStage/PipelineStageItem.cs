using System;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;

namespace PipelineLauncher.PipelineStage
{
    internal abstract class NoneResultStageItem<TItem> : PipelineStageItem<TItem>
    {
        public object OriginalItem { get; }
        public Type StageType { get; }

        protected NoneResultStageItem(object originalItem, Type stageType) : base(default)
        {
            StageType = stageType;
            OriginalItem = originalItem;

        }

        public abstract NoneResultStageItem<TNewItem> Return<TNewItem>();
    }

    internal class RemoveStageItem<TItem> : NoneResultStageItem<TItem>
    {
        public RemoveStageItem(object originalItem, Type stageType): base(originalItem, stageType)
        {
        }

        public override NoneResultStageItem<TNewItem> Return<TNewItem>()
        {
            return new RemoveStageItem<TNewItem>(OriginalItem, StageType);
        }
    }

    internal class SkipStageItem<TItem> : NoneResultStageItem<TItem>
    {
        public SkipStageItem(object originalItem, Type stageType) : base(originalItem, stageType)
        {
        }

        public override NoneResultStageItem<TNewItem> Return<TNewItem>()
        {
            return new SkipStageItem<TNewItem>(OriginalItem, StageType);
        }
    }

    internal class SkipStageItemTill<TItem> : NoneResultStageItem<TItem>
    {
        public Type SkipTillType { get; }

        public SkipStageItemTill(Type skipTillType, object originalItem, Type stageType) : base(originalItem, stageType)
        {
            SkipTillType = skipTillType;
        }

        public override NoneResultStageItem<TNewItem> Return<TNewItem>()
        {
            return new SkipStageItemTill<TNewItem>(SkipTillType, OriginalItem, StageType);
        }
    }

    internal class ExceptionStageItem<TItem> : NoneResultStageItem<TItem>
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

        public override NoneResultStageItem<TNewItem> Return<TNewItem>()
        {
            return new ExceptionStageItem<TNewItem>(Exception, Retry, StageType, FailedItems);
        }
    }
}