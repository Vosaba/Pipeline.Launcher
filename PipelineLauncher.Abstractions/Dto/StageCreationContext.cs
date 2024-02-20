using System;
using System.Threading;
using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Abstractions.PipelineStage.Dto;

namespace PipelineLauncher.Abstractions.Dto
{
    public class StageCreationContext
    {
        public event DiagnosticEventHandler DiagnosticEvent;

        private Action<ExceptionItemsEventArgs> _instantExceptionHandler;

        public CancellationToken CancellationToken { get; private set; }
        public PipelineType PipelineType { get; }
        public bool UseTimeOut { get; }

        public StageCreationContext(PipelineType pipelineType, bool useTimeOut)
        {
            PipelineType = pipelineType;
            UseTimeOut = useTimeOut;
        }

        public void SetupInstantExceptionHandler(Action<ExceptionItemsEventArgs> exceptionHandler)
        {
            _instantExceptionHandler = exceptionHandler;
        }

        public void SetupCancellationToken(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
        }

        public StageExecutionContext GetPipelineStageContext(Action retry)
        {
            return new StageExecutionContext(CancellationToken, new ActionsSet(retry, _instantExceptionHandler, DiagnosticEvent));
        }
    }
}