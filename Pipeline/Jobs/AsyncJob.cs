using PipelineLauncher.PipelineJobs;
using System;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Attributes;
using PipelineLauncher.Exceptions;

namespace PipelineLauncher.Jobs
{
    /// <summary>
    /// 
    /// Represents a Job to be processed in the pipeline.
    /// 
    /// </summary>
    /// <typeparam name="TInput">The type of the param.</typeparam>
    /// <typeparam name="TOutput">The type of the result.</typeparam>
    public abstract class AsyncJob<TInput, TOutput> : PipelineJobAsync<TInput, TOutput>
    {
        /// <summary>
        /// Performs the job using the specified param.
        /// </summary>
        /// <param name="param">The param.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<TOutput> PerformAsync(TInput param, CancellationToken cancellationToken)
        {
            return await PerformAsync(param);
        }

        public virtual Task<TOutput> PerformAsync(TInput param)
        {
            return Task.FromResult(Perform(param));
        }

        public virtual TOutput Perform(TInput param)
        {
            throw new NotImplementedException($"Neither of {nameof(Perform)} methods, are not implemented");
        }

        public TOutput Keep()
        {
            throw new NonParamException(new KeepResult());
        }

        public TOutput Remove()
        {
            throw new NonParamException(new RemoveResult());
        }

        public TOutput Skip()
        {
            throw new NonParamException(new SkipResult());
        }

        public TOutput SkipTo<TSkipToJob>() where TSkipToJob : IPipelineJob
        {
            throw new NonParamException(new SkipToResult(typeof(TSkipToJob)));
        }
    }

    public abstract class AsyncJob<TInput> : AsyncJob<TInput, TInput>
    {
    }
}
