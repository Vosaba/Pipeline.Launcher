namespace PipelineLauncher.Abstractions.PipelineStage.Configurations
{
    public class BulkStageConfiguration : StageBaseConfiguration
    {
        public int BatchItemsCount { get; set; } = 100;
        public int BatchItemsTimeOut { get; set; } = 100;
    }
}
