using System;

namespace PipelineLauncher.Abstractions.PipelineStage.Configuration
{
    public class PipelineBaseConfiguration
    {
        public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;
        public int MaxMessagesPerTask { get; set; } = 1;
        public bool SingleProducerConstrained { get; set; }
    }
}
