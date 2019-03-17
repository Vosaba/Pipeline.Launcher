using PipelineLauncher.PipelineJobs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PipelineLauncher.Jobs
{
    public abstract class Job<TInput, TOutput> : PipelineJobSync<TInput, TOutput>
    {
        public override async Task<IEnumerable<TOutput>> PerformAsync(TInput[] param, CancellationToken cancellationToken)
        {
            return await PerformAsync(param);
        }

         public virtual Task<IEnumerable<TOutput>> PerformAsync(TInput[] param)
         {
             return Task.FromResult(Perform(param));
         }

        public virtual IEnumerable<TOutput> Perform(TInput[] param)
        {
            throw new NotImplementedException($"Neither of {nameof(Perform)} methods, are not implemented");
        }
    }

    public abstract class Job<TInput> : Job<TInput, TInput>
    {}
}