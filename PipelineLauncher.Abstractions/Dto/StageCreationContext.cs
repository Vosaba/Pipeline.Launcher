using System;
using System.Threading;
using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Abstractions.PipelineStage.Dto;

namespace PipelineLauncher.Abstractions.Dto
{
    public class StageCreationContext
    {
        public event DiagnosticEventHandler DiagnosticEvent;

        private Action<ExceptionItemsEventArgs> _exceptionHandler;

        public CancellationToken CancellationToken { get; private set; }
        public PipelineType PipelineType { get; }
        public bool UseTimeOuts { get; }

        public StageCreationContext(PipelineType pipelineType, bool useTimeOuts)
        {
            PipelineType = pipelineType;
            UseTimeOuts = useTimeOuts;
        }

        public void SetupExceptionHandler(Action<ExceptionItemsEventArgs> exceptionHandler)
        {
            _exceptionHandler = exceptionHandler;
        }

        public void SetupCancellationToken(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
        }

        public StageExecutionContext GetPipelineStageContext(Action retry)
        {
            return new StageExecutionContext(CancellationToken, new ActionsSet(retry, _exceptionHandler, DiagnosticEvent));
        }
    }
}