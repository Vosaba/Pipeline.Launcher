using System;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.PipelineStage;

namespace PipelineLauncher.Exceptions
{
    internal class NoneParamException<TItem> : Exception
    {
        public NoneResultStageItem<TItem> StageItem { get; }
        public NoneParamException(NoneResultStageItem<TItem> stageItem)
        {
            StageItem = stageItem;
        }
    }
}
