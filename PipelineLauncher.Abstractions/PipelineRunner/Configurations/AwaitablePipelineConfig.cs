namespace PipelineLauncher.Abstractions.PipelineRunner.Configurations
{
    public class AwaitablePipelineConfig : PipelineConfig
    {
        public bool IgnoreTimeOuts { get; set; } = true;
        public bool ThrowExceptionOccured { get; set; }
    }
}