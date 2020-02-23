using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Stages;

namespace PipelineLauncher.Stages
{
    /// <summary>
    /// 
    /// Represents a Stage to be processed in the pipeline.
    /// 
    /// </summary>
    /// <typeparam name="TInput">The type of the param.</typeparam>
    /// <typeparam name="TOutput">The type of the result.</typeparam>
    public abstract class Stage<TInput, TOutput> : IStage<TInput, TOutput>
    {
        public virtual StageConfiguration Configuration => new StageConfiguration();

        /// <summary>
        /// Performs the Stage using the specified param.
        /// </summary>
        /// <param name="input">The param.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public virtual async Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(input);
        }

        [DebuggerStepThrough]
        public virtual Task<TOutput> ExecuteAsync(TInput input)
        {
            return Task.FromResult(Execute(input));
        }

        [DebuggerStepThrough]
        public virtual TOutput Execute(TInput input)
        {
            throw new NotImplementedException($"Neither of {nameof(Execute)} methods, are not implemented");
        }

        protected TOutput Remove(TInput input)
        {
            return new StageOption<TInput, TOutput>().Remove(input);
        }

        protected TOutput Skip(TInput input)
        {
            return new StageOption<TInput, TOutput>().Skip(input);
        }

        protected TOutput SkipTo<TTargetStage>(TInput input) where TTargetStage : ITargetStage<TInput>
        {
            return new StageOption<TInput, TOutput>().SkipTo<TTargetStage>(input);
        }
    }

    public abstract class Stage<TInput> : Stage<TInput, TInput>
    {
    }

    public abstract class ConditionalStage<TInput, TOutput> : Stage<TInput, TOutput>, IConditionalStage<TInput>
    {
        public abstract PredicateResult Predicate(TInput input);
    }

    public abstract class ConditionalStage<TInput> : Stage<TInput>, IConditionalStage<TInput>
    {
        public abstract PredicateResult Predicate(TInput input);
    }
}
