using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using PipelineLauncher.PipelineStage;

namespace PipelineLauncher.Stages
{
    public abstract class BulkStage<TInput, TOutput> : PipelineBulkStage<TInput, TOutput>
    {
        public override BulkStageConfiguration Configuration => new BulkStageConfiguration();

        [DebuggerStepThrough]
        public override async Task<IEnumerable<TOutput>> ExecuteAsync(IEnumerable<TInput> input, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(input);
        }

        [DebuggerStepThrough]
        public virtual Task<IEnumerable<TOutput>> ExecuteAsync(IEnumerable<TInput> input)
         {
             return Task.FromResult(Execute(input));
         }

        [DebuggerStepThrough]
        public virtual IEnumerable<TOutput> Execute(IEnumerable<TInput> param)
        {
            throw new NotImplementedException($"Neither of {nameof(Execute)} methods, are not implemented");
        }
    }

    public abstract class BulkStage<TInput> : BulkStage<TInput, TInput>
    {}
}