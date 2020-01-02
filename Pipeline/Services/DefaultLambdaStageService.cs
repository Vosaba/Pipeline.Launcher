using System;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.Abstractions.Stages;

namespace PipelineLauncher.Services
{
    public class DefaultLambdaStageService : IStageService
    {
        private Func<Type, IStage> _stageResolver;

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