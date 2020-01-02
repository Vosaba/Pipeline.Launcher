using PipelineLauncher.PipelineStage;
using System;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.PipelineStage.Configuration;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
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
    public abstract class Stage<TInput, TOutput> : PipelineStage<TInput, TOutput>
    {
        public override StageConfiguration Configuration => new StageConfiguration();

        /// <summary>
        /// Performs the Stage using the specified param.
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
            return new StageOption<TInput, TOutput>().Remove(input);
        }

        public TOutput Skip(TInput input)
        {
            return new StageOption<TInput, TOutput>().Skip(input);
        }

        public TOutput SkipTo<TSkipToStage>(TInput input) where TSkipToStage : IStageIn<TInput>
        {
            return new StageOption<TInput, TOutput>().SkipTo<TSkipToStage>(input);
        }
    }

    public abstract class Stage<TInput> : Stage<TInput, TInput>
    {
    }
}
