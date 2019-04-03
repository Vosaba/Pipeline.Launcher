using System.Collections.Generic;
using System.Threading;

namespace PipelineLauncher.Dataflow
{
    public interface IConsumingEnumerable<out T>
    {
        IEnumerable<T> GetConsumingEnumerable();
        IEnumerable<T> GetConsumingEnumerable(CancellationToken cancellationToken);
    }
}
