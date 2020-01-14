using System;

namespace PipelineLauncher.Abstractions.PipelineStage.Configurations
{
    public class PipelineBaseConfiguration
    {
        public int MaxDegreeOfParallelism { get; set; } = 1;// Environment.ProcessorCount;
        public int MaxMessagesPerTask { get; set; } = 1;
        public bool SingleProducerConstrained { get; set; }
    }
}
