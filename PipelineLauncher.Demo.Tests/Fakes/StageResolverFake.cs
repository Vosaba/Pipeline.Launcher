using PipelineLauncher.Abstractions.Services;
using System;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.Stages;

namespace PipelineLauncher.Demo.Tests.Fakes
{
    public class StageServiceFake : IStageService
    {
        public TStage GetStageInstance<TStage>() where TStage : class, IStage
        {
            return (TStage)Activator.CreateInstance(typeof(TStage));
        }
    }
}
