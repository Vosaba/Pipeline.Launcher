﻿using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;

namespace PipelineLauncher.StageSetup
{
    public interface IStageSetupIn<TInput> : IStageSetup
    {
        new ITargetBlock<PipelineStageItem<TInput>> RetrieveExecutionBlock(StageCreationContext context);
    }
}
