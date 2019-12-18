using System;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Configurations;
using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.Abstractions.Pipeline
{
    public interface IPipelineJob<TInput, TOutput> : IPipeline<TInput, TOutput>
    {
        Task<PipelineItem<TOutput>> InternalExecute(PipelineItem<TInput> input, Action reExecute, CancellationToken cancellationToken);
        JobConfiguration Configuration { get; }
    }
}