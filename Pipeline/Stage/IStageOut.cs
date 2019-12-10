using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.Stage
{
    public interface IStageOut<TOut> : IStage
    {
        new ISourceBlock<PipelineItem<TOut>> ExecutionBlock { get; }
    }
}