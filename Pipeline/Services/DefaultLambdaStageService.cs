using System;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Abstractions.Services;

namespace PipelineLauncher.Services
{
    public class DefaultLambdaStageService : IStageService
    {
        private Func<Type, IPipeline> _stageResolver;

        public DefaultLambdaStageService(Func<Type, IPipeline> stageResolver)
        {
            _stageResolver = stageResolver;
        }

        public TPipelineStage GetStageInstance<TPipelineStage>() where TPipelineStage : class, IPipeline
        {
            return (TPipelineStage) _stageResolver(typeof(TPipelineStage));
        }
    }
}