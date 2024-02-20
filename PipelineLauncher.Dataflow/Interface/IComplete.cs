using System.Threading.Tasks;

namespace PipelineLauncher.Dataflow
{
    public interface IComplete
    {
        void CompleteAdding();
        bool IsCompleted { get; }
        Task ExecutionTask { get; }
    }
}