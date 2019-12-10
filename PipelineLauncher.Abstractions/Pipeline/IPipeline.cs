namespace PipelineLauncher.Abstractions.Pipeline
{
    public interface IPipelineJob
    {
    }

    public interface IPipelineJobIn<TInput>: IPipelineJob
    {
    }

    public interface IPipelineJobOut<TOutput>: IPipelineJob
    {
    }

    public interface IPipelineJob<TInput, TOutput>: IPipelineJobIn<TInput>, IPipelineJobOut<TOutput>
    {
        int MaxDegreeOfParallelism { get; }
    }
}
