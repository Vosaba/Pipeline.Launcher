using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.Stage
{
    public interface IStageIn<TIn> : IStage
    {
        new ITargetBlock<PipelineItem<TIn>> ExecutionBlock { get; }
    }
}
