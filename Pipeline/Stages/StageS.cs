using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Stages;
using PipelineLauncher.Exceptions;
using PipelineLauncher.PipelineStage;

namespace PipelineLauncher.Stages
{
    /// <summary>
    /// 
    /// Represents a Stage to be processed in the pipeline.
    /// 
    /// </summary>
    /// <typeparam name="TInput">The type of the param.</typeparam>
    /// <typeparam name="TOutput">The type of the result.</typeparam>
    public abstract class StageS<TInput, TOutput> : IStage<TInput, TOutput>
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
            throw new NonResultStageItemException<TOutput>(new RemoveStageItem<TOutput>(input, GetType()));
        }

        protected TOutput Skip(TInput input)
        {
            throw new NonResultStageItemException<TOutput>(new SkipStageItem<TOutput>(input, GetType(), true));
        }

        protected TOutput SkipTo<TTargetStage>(TInput input) where TTargetStage : ITargetStage<TInput>
        {
            throw new NonResultStageItemException<TOutput>(new SkipStageItemTill<TOutput>(typeof(TTargetStage), input, GetType()));
        }
    }

    public abstract class StageS<TInput> : StageS<TInput, TInput>
    {
    }

    public abstract class ConditionalStageS<TInput, TOutput> : StageS<TInput, TOutput>, IConditionalStage<TInput>
    {
        public abstract PredicateResult Predicate(TInput input);
    }

    public abstract class ConditionalStageS<TInput> : StageS<TInput>, IConditionalStage<TInput>
    {
        public abstract PredicateResult Predicate(TInput input);
    }
}
