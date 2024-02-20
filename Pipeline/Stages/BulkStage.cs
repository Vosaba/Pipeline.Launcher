using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Stages;

namespace PipelineLauncher.Stages
{
    public abstract class BulkStage<TInput, TOutput> : IBulkStage<TInput, TOutput>
    {
        public virtual BulkStageConfiguration Configuration => new BulkStageConfiguration();

        [DebuggerStepThrough]
        public async Task<IEnumerable<TOutput>> ExecuteAsync(TInput[] input, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(input);
        }

        [DebuggerStepThrough]
        public virtual Task<IEnumerable<TOutput>> ExecuteAsync(TInput[] input)
         {
             return Task.FromResult(Execute(input));
         }

        [DebuggerStepThrough]
        public virtual IEnumerable<TOutput> Execute(TInput[] input)
        {
            throw new NotImplementedException($"Neither of {nameof(Execute)} methods, are not implemented");
        }
    }

    public abstract class BulkStage<TInput> : BulkStage<TInput, TInput>
    {}

    public abstract class ConditionalBulkStage<TInput, TOutput> : BulkStage<TInput, TOutput>, IConditionalStage<TInput>
    {
        public abstract PredicateResult Predicate(TInput input);
    }

    public abstract class ConditionalBulkStage<TInput> : BulkStage<TInput>, IConditionalStage<TInput>
    {
        public abstract PredicateResult Predicate(TInput input);
    }
}