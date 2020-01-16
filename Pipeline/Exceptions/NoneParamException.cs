using System;
using PipelineLauncher.Abstractions.PipelineStage;

namespace PipelineLauncher.Exceptions
{
    internal class NoneParamException<TItem> : Exception
    {
        public PipelineStageItem<TItem> StageItem { get; }
        public NoneParamException(PipelineStageItem<TItem> stageItem)
        {
            StageItem = stageItem;
        }
    }
}
