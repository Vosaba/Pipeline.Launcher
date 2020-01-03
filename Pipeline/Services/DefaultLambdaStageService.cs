using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.Abstractions.Stages;
using System;

namespace PipelineLauncher.Services
{
    public class DefaultLambdaStageService : IStageService
    {
        private readonly Func<Type, IStage> _stageResolver;

        public DefaultLambdaStageService(Func<Type, IStage> stageResolver)
        {
            _stageResolver = stageResolver;
        }

        public TPipelineStage GetStageInstance<TPipelineStage>() where TPipelineStage : class, IStage
        {
            return (TPipelineStage) _stageResolver(typeof(TPipelineStage));
        }
    }
}