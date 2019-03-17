using PipelineLauncher.Abstractions.Pipeline;

namespace PipelineLauncher.Stages
{
    public interface IStage
    {
        IPipelineJob Job { get; }
        IStage Next { get; set; }
        IStage Previous { get; set; }
    }
}
