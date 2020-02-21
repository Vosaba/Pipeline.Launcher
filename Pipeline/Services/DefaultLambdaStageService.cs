using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.Abstractions.Stages;
using System;
using PipelineLauncher.Abstractions.PipelineStage;

namespace PipelineLauncher.Services
{
    public class DefaultLambdaStageService : IStageService
    {
        private readonly Func<Type, IPipelineStage> _stageResolver;

        public DefaultLambdaStageService(Func<Type, IPipelineStage> stageResolver)
        {
            _stageResolver = stageResolver;
        }

        public TStage GetStageInstance<TStage>() where TStage : class, IStage
        {
            return (TStage) _stageResolver(typeof(TStage));
        }
    }
}