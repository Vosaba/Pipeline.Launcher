using System;
using System.Threading;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.Abstractions.Stages;
using PipelineLauncher.Services;

namespace PipelineLauncher.PipelineSetup
{
    internal class PipelineCreationContext
    {
        private IStageService _stageService;
        //private Action<DiagnosticItem> _diagnosticHandler;
        //private Action<ExceptionItemsEventArgs> _exceptionHandler;

        //public event DiagnosticEventHandler DiagnosticEvent;
        //public TaskContinuationOptions TaskContinuationOptions = TaskContinuationOptions.ExecuteSynchronously;

        public bool UseDefaultServiceResolver { get; private set; } = true;

        //public CancellationToken CancellationToken { get; private set; }

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

        public PipelineCreationContext(Func<Type, IStage> stageResolveFunc)
        {
            _stageService = new DefaultLambdaStageService(stageResolveFunc);
        }

        public PipelineCreationContext SetupStageService(IStageService stageService)
        {
            _stageService = stageService;
            return this;
        }

        public PipelineCreationContext SetupStageService(Func<Type, IStage> stageResolveFunc)
        {
            _stageService = new DefaultLambdaStageService(stageResolveFunc);
            return this;
        }

        //public PipelineCreationContext SetupExceptionHandler(Action<ExceptionItemsEventArgs> exceptionHandler)
        //{
        //    _exceptionHandler = exceptionHandler;
        //    return this;
        //}

        public PipelineCreationContext SetupConfiguration(bool useDefaultServiceResolver)
        {
            UseDefaultServiceResolver = useDefaultServiceResolver;
            return this;
        }

        //public PipelineCreationContext SetupDiagnosticAction(Action<DiagnosticItem> diagnosticAction)
        //{
        //    _diagnosticHandler = diagnosticAction;
        //    return this;
        //}

        //public PipelineCreationContext SetupCancellationToken(CancellationToken cancellationToken)
        //{
        //    CancellationToken = cancellationToken;
        //    return this;
        //}

        public StageExecutionContext GetPipelineStageContext(Action retry, CancellationToken cancellationToken, Action<ExceptionItemsEventArgs> exceptionHandler, DiagnosticEventHandler diagnosticHandler)
        {
            return new StageExecutionContext(cancellationToken, new ActionsSet(retry, exceptionHandler, diagnosticHandler));
        }
    }
}