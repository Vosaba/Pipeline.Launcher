using System.Threading;

namespace PipelineLauncher.Abstractions.PipelineStage.Dto
{
    public class PipelineStageContext
    {
        public int ExecutionTryCount { get; private set; } = 0;
        public CancellationToken CancellationToken { get; }
        public ActionsSet ActionsSet { get; }

        public PipelineStageContext(CancellationToken cancellationToken, ActionsSet actionsSet)
        {
            CancellationToken = cancellationToken;
            ActionsSet = actionsSet;
        }

        public void AddExecutionTryCount()
        {
            ExecutionTryCount++;
        }
    }
}