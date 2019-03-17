using System.Threading;

namespace PipelineLauncher.Abstractions.Pipeline
{
    public interface IPipelineJobAsync : IPipelineJob
    {
        object InternalPerform(object param, CancellationToken cancellationToken);

        int MaxDegreeOfParallelism { get; }
    }
}