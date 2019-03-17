using System.Collections.Generic;
using System.Threading;

namespace PipelineLauncher.Abstractions.Pipeline
{
    public interface IPipelineJobSync : IPipelineJob
    {
        IEnumerable<object> InternalPerform(object[] param, CancellationToken cancellationToken);
    }
}