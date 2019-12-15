using PipelineLauncher.Abstractions.Configurations;
using PipelineLauncher.PipelineJobs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PipelineLauncher.Jobs
{
    public abstract class Job<TInput, TOutput> : PipelineJobSync<TInput, TOutput>
    {
        public override JobConfiguration Configuration => new JobConfiguration();


        public override async Task<IEnumerable<TOutput>> ExecuteAsync(IEnumerable<TInput> input, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(input);
        }

         public virtual Task<IEnumerable<TOutput>> ExecuteAsync(IEnumerable<TInput> input)
         {
             return Task.FromResult(Execute(input));
         }

        public virtual IEnumerable<TOutput> Execute(IEnumerable<TInput> param)
        {
            throw new NotImplementedException($"Neither of {nameof(Execute)} methods, are not implemented");
        }
    }

    public abstract class Job<TInput> : Job<TInput, TInput>
    {}
}