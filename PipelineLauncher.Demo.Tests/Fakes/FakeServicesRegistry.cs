using PipelineLauncher.Abstractions.Services;
using System;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.Stages;

namespace PipelineLauncher.Demo.Tests.Fakes
{
    class FakeServicesRegistry
    {
        public class StageService : IStageService
        {
            public TPipelineStage GetStageInstance<TPipelineStage>() where TPipelineStage : class, IStage
            {
                return (TPipelineStage)Activator.CreateInstance(typeof(TPipelineStage));
            }
        }
    }
}
