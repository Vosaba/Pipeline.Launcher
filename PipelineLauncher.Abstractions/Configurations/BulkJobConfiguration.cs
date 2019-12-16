using System.Collections.Generic;
using System.Text;

namespace PipelineLauncher.Abstractions.Configurations
{

    public class BulkJobConfiguration : PipelineBaseConfiguration
    {
        public int BatchItemsCount { get; set; } = 2;
        public int BatchItemsTimeOut { get; set; } = 100;
    }

    public class JobConfiguration : PipelineBaseConfiguration
    {

    }
}
