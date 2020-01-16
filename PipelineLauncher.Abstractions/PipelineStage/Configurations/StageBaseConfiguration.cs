using System;

namespace PipelineLauncher.Abstractions.PipelineStage.Configurations
{
    public class StageBaseConfiguration
    {
        public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;
        public int MaxMessagesPerTask { get; set; } = 1;
        public bool SingleProducerConstrained { get; set; }

        public int MaxRetriesCount { get; set; } = 2;
    }
}
