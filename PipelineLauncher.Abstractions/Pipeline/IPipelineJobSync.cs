using System.Collections.Generic;
using System.Threading;

namespace PipelineLauncher.Abstractions.Pipeline
{
    public interface IPipelineJobSync : IPipelineJob
    {
        IEnumerable<object> InternalExecute(IEnumerable<object> input, CancellationToken cancellationToken);
    }
}