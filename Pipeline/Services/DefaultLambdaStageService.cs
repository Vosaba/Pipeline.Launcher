using PipelineLauncher.Abstractions.Services;
using System;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.Stages;

namespace PipelineLauncher.Services
{
    public class DefaultLambdaStageService : IStageService
    {
        private readonly Func<Type, IStage> _stageResolver;

        public DefaultLambdaStageService(Func<Type, IStage> stageResolver)
        {
            _stageResolver = stageResolver;
        }

        public TStage GetStageInstance<TStage>() where TStage : class, IStage
        {
            return (TStage) _stageResolver(typeof(TStage));
        }
    }
}