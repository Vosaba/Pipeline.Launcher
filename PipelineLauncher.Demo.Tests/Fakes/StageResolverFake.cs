using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.Abstractions.Stages;
using System;
using PipelineLauncher.Abstractions.PipelineStage;

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
