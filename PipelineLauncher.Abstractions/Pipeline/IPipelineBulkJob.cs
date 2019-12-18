using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Configurations;
using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.Abstractions.Pipeline
{
    public interface IPipelineBulkJob<TInput, TOutput> : IPipeline<TInput, TOutput>
    {
        Task<IEnumerable<PipelineItem<TOutput>>> InternalExecute(IEnumerable<PipelineItem<TInput>> input, Action reExecute, CancellationToken cancellationToken);

        BulkJobConfiguration Configuration { get; }
    }
}