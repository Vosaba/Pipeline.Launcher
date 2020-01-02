using System;
using System.Threading;

namespace PipelineLauncher.Abstractions.Dto
{
    public class PipelineJobContext
    {
        public CancellationToken CancellationToken { get; }
        public ActionsSet ActionsSet { get; }

        public PipelineJobContext(CancellationToken cancellationToken, ActionsSet actionsSet)
        {
            CancellationToken = cancellationToken;
            ActionsSet = actionsSet;
        }
    }
}