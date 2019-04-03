using PipelineLauncher.PipelineJobs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PipelineLauncher.Jobs
{
    public abstract class Job<TInput, TOutput> : PipelineJobSync<TInput, TOutput>
    {
        public override async Task<IEnumerable<TOutput>> ExecuteAsync(TInput[] input, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(input);
        }

         public virtual Task<IEnumerable<TOutput>> ExecuteAsync(TInput[] input)
         {
             return Task.FromResult(Execute(input));
         }

        public virtual IEnumerable<TOutput> Execute(TInput[] param)
        {
            throw new NotImplementedException($"Neither of {nameof(Execute)} methods, are not implemented");
        }
    }

    public abstract class Job<TInput> : Job<TInput, TInput>
    {}

    public abstract class JobVariant<TInput, TOutput> : Job<TInput, TOutput>
    {
        public abstract bool Condition(TInput input);
    }

    public abstract class JobVariant<TInput> : JobVariant<TInput, TInput>
    {

    }
}