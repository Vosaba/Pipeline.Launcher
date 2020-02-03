using System;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.Abstractions.Stages;
using PipelineLauncher.Services;

namespace PipelineLauncher.PipelineSetup
{
    internal class PipelineSetupContext
    {
        private IStageService _stageService;
        private Action<DiagnosticItem> _diagnosticHandler;
        private Action<ExceptionItemsEventArgs> _exceptionHandler;

        //public TaskContinuationOptions TaskContinuationOptions = TaskContinuationOptions.ExecuteSynchronously;

        public bool UseDefaultServiceResolver { get; private set; } = true;

        public CancellationToken CancellationToken { get; private set; }

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

        public PipelineSetupContext()
        {
        }

        public  PipelineSetupContext(IStageService stageService)
        {
            _stageService = stageService;
        }

        public PipelineSetupContext(Func<Type, IStage> stageResolveFunc)
        {
            _stageService = new DefaultLambdaStageService(stageResolveFunc);
        }

        public PipelineSetupContext SetupStageService(IStageService stageService)
        {
            _stageService = stageService;
            return this;
        }

        public PipelineSetupContext SetupStageService(Func<Type, IStage> stageResolveFunc)
        {
            _stageService = new DefaultLambdaStageService(stageResolveFunc);
            return this;
        }

        public PipelineSetupContext SetupExceptionHandler(Action<ExceptionItemsEventArgs> exceptionHandler)
        {
            _exceptionHandler = exceptionHandler;
            return this;
        }

        public PipelineSetupContext SetupConfiguration(bool useDefaultServiceResolver)
        {
            UseDefaultServiceResolver = useDefaultServiceResolver;
            return this;
        }

        public PipelineSetupContext SetupDiagnosticAction(Action<DiagnosticItem> diagnosticAction)
        {
            _diagnosticHandler = diagnosticAction;
            return this;
        }

        public PipelineSetupContext SetupCancellationToken(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
            return this;
        }

        public PipelineStageContext GetPipelineStageContext(Action retry)
        {
            return new PipelineStageContext(
                CancellationToken, 
                new ActionsSet(retry, _exceptionHandler, _diagnosticHandler));
        }
    }
}