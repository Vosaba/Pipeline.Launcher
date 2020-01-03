namespace PipelineLauncher.Abstractions.PipelineStage.Configurations
{
    public class BulkStageConfiguration : PipelineBaseConfiguration
    {
        public int BatchItemsCount { get; set; } = 100;
        public int BatchItemsTimeOut { get; set; } = 100;
    }
}
