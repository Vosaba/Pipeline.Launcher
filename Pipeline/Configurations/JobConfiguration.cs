using System;
using System.Collections.Generic;
using System.Text;

namespace PipelineLauncher.Configurations
{
    public class PipelineBaseConfiguration
    {
        public int MaxDegreeOfParallelism => Environment.ProcessorCount;
        public bool SingleProducerConstrained { get; set; }
    }

    public class JobConfiguration : PipelineBaseConfiguration
    {
        public int BatchItemsCount { get; }
        public int BatchItemsTimeOut { get; }
    }

    public class JobAsyncConfiguration : PipelineBaseConfiguration
    {

    }
}
