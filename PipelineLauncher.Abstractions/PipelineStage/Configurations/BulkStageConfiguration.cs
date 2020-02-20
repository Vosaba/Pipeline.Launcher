namespace PipelineLauncher.Abstractions.PipelineStage.Configurations
{
    public class BulkStageConfiguration : StageBaseConfiguration
    {
        public int BatchSize { get; set; } = 10;
        public int BatchTimeOut { get; set; } = 100;
    }
}
