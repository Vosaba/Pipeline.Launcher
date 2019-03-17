using System;

namespace PipelineLauncher.Attributes
{
    public class PipelineFilterResult {}

    internal class KeepResult : PipelineFilterResult {}

    internal class RemoveResult : PipelineFilterResult {}

    internal class SkipResult : PipelineFilterResult {}

    internal class SkipToResult : PipelineFilterResult
    {
        public Type JobType { get; }

        public SkipToResult(Type jobType)
        {
            JobType = jobType;
        }
    }
}