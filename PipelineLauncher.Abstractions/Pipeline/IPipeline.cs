using System;
using PipelineLauncher.Abstractions.Collections;

namespace PipelineLauncher.Abstractions.Pipeline
{
    public interface IPipelineJobIn<TInput>
    {
    }

    public interface IPipelineJobOut<TOutput>
    {

    }

    public interface IPipelineJob
    {
        Type AcceptedType { get; }
    }

    public interface IPipelineJob<TInput, TOutput>: IPipelineJob, IPipelineJobIn<TInput>, IPipelineJobOut<TOutput>
    {
        //Type AcceptedType { get; }

        int MaxDegreeOfParallelism { get; }
    }
}
