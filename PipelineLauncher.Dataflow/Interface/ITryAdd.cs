using System.Threading;

namespace PipelineLauncher.Dataflow
{
    public interface ITryAdd<in T>
    {
        bool TryAdd(T item);
        bool TryAdd(T item, int timeout);
        bool TryAdd(T item, int timeout, CancellationToken cancellationToken);
    }
}