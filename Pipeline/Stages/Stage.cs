using PipelineLauncher.Abstractions.Pipeline;

namespace PipelineLauncher.Stages
{
    internal class Stage : IStage
    {
        public Stage(IPipelineJob job)
        {
            Job = job;
        }

        public IPipelineJob Job { get; }

        public IStage Next { get; set; }

        public IStage Previous { get; set; }
    }
}
