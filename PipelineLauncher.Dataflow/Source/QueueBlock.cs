using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace PipelineLauncher.Dataflow
{
    public class QueueBlock<TIn> : BlockingCollection<TIn>, IDataBlockCollection<TIn>
    {
        public Task ExecutionTask { get; }
    }
}
