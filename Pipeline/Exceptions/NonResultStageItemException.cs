using System;
using PipelineLauncher.PipelineStage;

namespace PipelineLauncher.Exceptions
{
    internal class NonResultStageItemException<TItem> : Exception
    {
        public NonResultStageItem<TItem> StageItem { get; }
        public NonResultStageItemException(NonResultStageItem<TItem> stageItem)
        {
            StageItem = stageItem;
        }
    }
}
