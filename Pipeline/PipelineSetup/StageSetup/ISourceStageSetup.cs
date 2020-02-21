﻿using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;
using System.Threading.Tasks.Dataflow;

namespace PipelineLauncher.PipelineSetup.StageSetup
{
    public interface ISourceStageSetup<TOutput> : IStageSetup
    {
        new ISourceBlock<PipelineStageItem<TOutput>> RetrieveExecutionBlock(StageCreationContext context);
    }
}