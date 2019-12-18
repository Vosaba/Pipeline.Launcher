namespace PipelineLauncher.Abstractions.Pipeline
{
    public interface IPipelineJob
    {
    }

    public interface IPipelineIn<TInput>: IPipelineJob
    {
    }

    public interface IPipelineOut<TOutput>: IPipelineJob
    {
    }

    public interface IPipeline<TInput, TOutput>: IPipelineIn<TInput>, IPipelineOut<TOutput>
    {
    }
}
