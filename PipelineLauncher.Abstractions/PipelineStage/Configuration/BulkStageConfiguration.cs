using PipelineLauncher.Abstractions.PipelineStage.Dto;

namespace PipelineLauncher.Abstractions.PipelineStage.Configuration
{
    public class BulkStageConfiguration : PipelineBaseConfiguration
    {
        public int BatchItemsCount { get; set; } = 2;
        public int BatchItemsTimeOut { get; set; } = 100;
    }
}
