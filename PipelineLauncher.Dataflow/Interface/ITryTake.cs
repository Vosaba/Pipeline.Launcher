using System.Threading;

namespace PipelineLauncher.Dataflow
{
    public interface ITryTake<T>
    {
        bool TryTake(out T item);
        bool TryTake(out T item, int timeout);
        bool TryTake(out T item, int timeout, CancellationToken cancellationToken);
    }
}