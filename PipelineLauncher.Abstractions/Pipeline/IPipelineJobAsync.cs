using System;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.Abstractions.Pipeline
{
    public interface IPipelineJobAsync<TInput, TOutput> : IPipelineJob<TInput, TOutput>
    {
        Task<PipelineItem<TOutput>> InternalExecute(PipelineItem<TInput> input, Action reExecute, CancellationToken cancellationToken);
    }
}