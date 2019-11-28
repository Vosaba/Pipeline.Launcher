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
        /// <param name="input">The param.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(input);
        }

        public virtual Task<TOutput> ExecuteAsync(TInput input)
        {
            return Task.FromResult(Execute(input));
        }

        public virtual TOutput Execute(TInput input)
        {
            throw new NotImplementedException($"Neither of {nameof(Execute)} methods, are not implemented");
        }

        public TOutput Remove(TInput input)
        {
            throw new NonParamException<TOutput>(new RemoveItem<TOutput>(input, GetType()));
        }

        public TOutput Skip(TInput input)
        {
            throw new NonParamException<TOutput>(new SkipItem<TOutput>(input, GetType()));
        }

        public TOutput SkipTo<TSkipToJob>(TInput input) where TSkipToJob : IPipelineJobIn<TInput>
        {
            throw new NonParamException<TOutput>(new SkipItemTill<TOutput>(typeof(TSkipToJob), input, GetType()));
        }
    }

    public abstract class AsyncJob<TInput> : AsyncJob<TInput, TInput>
    {
    }
}
