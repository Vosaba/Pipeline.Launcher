namespace PipelineLauncher.Abstractions.Dto
{
    public class StageCreationOptions
    {
        public PipelineType PipelineType { get; }

        public bool UseTimeOuts => PipelineType == PipelineType.Normal;

        public StageCreationOptions(PipelineType pipelineType)
        {
            PipelineType = pipelineType;
        }


    }
}