using System;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.PipelineStage;

namespace PipelineLauncher.Exceptions
{
    internal class NoneParamException<TItem> : Exception
    {
        public NonResultStageItem<TItem> StageItem { get; }
        public NoneParamException(NonResultStageItem<TItem> stageItem)
        {
            StageItem = stageItem;
        }
    }
}
