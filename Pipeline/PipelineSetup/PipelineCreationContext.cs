using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.Abstractions.Stages;
using PipelineLauncher.Services;
using System;

namespace PipelineLauncher.PipelineSetup
{
    internal class PipelineCreationContext
    {
        private IStageService _stageService;
        public bool UseDefaultServiceResolver { get; private set; } = true;

        public IStageService StageService
        {
            get
            {
                if (_stageService == null)
                {
                    if (UseDefaultServiceResolver)
                    {
                        _stageService = new DefaultStageService();
                    }
                    else
                    {
                        throw new Exception($"'{nameof(IStageService)}' isn't provided, if you need to use Generic stage setups, provide service.");
                    }
                }

                return _stageService;
            }
        }

        public PipelineCreationContext()
        {
        }

        public  PipelineCreationContext(IStageService stageService)
        {
            _stageService = stageService;
        }

        public PipelineCreationContext(Func<Type, IPipelineStage> stageResolveFunc)
        {
            _stageService = new DefaultLambdaStageService(stageResolveFunc);
        }

        public PipelineCreationContext SetupStageService(IStageService stageService)
        {
            _stageService = stageService;
            return this;
        }

        public PipelineCreationContext SetupStageService(Func<Type, IPipelineStage> stageResolveFunc)
        {
            _stageService = new DefaultLambdaStageService(stageResolveFunc);
            return this;
        }

        public PipelineCreationContext SetupConfiguration(bool useDefaultServiceResolver)
        {
            UseDefaultServiceResolver = useDefaultServiceResolver;
            return this;
        }
    }
}