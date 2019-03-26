using System.Collections.Generic;
using System.Threading;

namespace PipelineLauncher.Abstractions.Pipeline
{
    public interface IPipelineJobSync : IPipelineJob
    {
        IEnumerable<object> InternalExecute(object[] input, CancellationToken cancellationToken);
    }
}