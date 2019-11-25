using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.Abstractions.Pipeline
{
    public interface IPipelineJobSync<TInput, TOutput> : IPipelineJob<TInput, TOutput>
    {
        Task<IEnumerable<PipelineItem<TOutput>>> InternalExecute(IEnumerable<PipelineItem<TInput>> input, CancellationToken cancellationToken);
    }
}