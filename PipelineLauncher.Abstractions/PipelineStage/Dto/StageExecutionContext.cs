using System.Threading;

namespace PipelineLauncher.Abstractions.PipelineStage.Dto
{
    public class StageExecutionContext
    {
        public CancellationToken CancellationToken { get; }
        public ActionsSet ActionsSet { get; }

        public StageExecutionContext(CancellationToken cancellationToken, ActionsSet actionsSet)
        {
            CancellationToken = cancellationToken;
            ActionsSet = actionsSet;
        }

    }
}