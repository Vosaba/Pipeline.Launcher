namespace PipelineLauncher.Abstractions.Pipeline
{
    public interface IPipeline
    {
    }

    public interface IPipelineIn<TInput>: IPipeline
    {
    }

    public interface IPipelineOut<TOutput>: IPipeline
    {
    }

    public interface IPipeline<TInput, TOutput>: IPipelineIn<TInput>, IPipelineOut<TOutput>
    {
    }
}
