namespace PipelineLauncher.Abstractions.PipelineRunner.Configurations
{
    public class AwaitablePipelineCreationConfig : PipelineCreationConfig
    {
        public bool IgnoreTimeOuts { get; set; } = true;
        public bool ThrowExceptionOccured { get; set; }
    }
}