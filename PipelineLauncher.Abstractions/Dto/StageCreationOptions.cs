namespace PipelineLauncher.Abstractions.Dto
{
    public class StageCreationOptions
    {
        public PipelineType PipelineType { get; }

        public bool UseTimeOuts { get; }

        public StageCreationOptions(PipelineType pipelineType, bool useTimeOuts)
        {
            PipelineType = pipelineType;
            UseTimeOuts = useTimeOuts;
        }
    }
}