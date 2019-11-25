using System.Threading;

namespace PipelineLauncher.Abstractions.Pipeline
{
    public interface IPipelineJobAsync : IPipelineJob
    {
        object InternalExecute(object input, CancellationToken cancellationToken);
    }
}