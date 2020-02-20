using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.Abstractions.Stages;
using System;

namespace PipelineLauncher.Demo.Tests.Fakes
{
    public class StageServiceFake : IStageService
    {
        public TPipelineStage GetStageInstance<TPipelineStage>() where TPipelineStage : class, IPipelineStage
        {
            return (TPipelineStage)Activator.CreateInstance(typeof(TPipelineStage));
        }
    }
}
