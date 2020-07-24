using System;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;

namespace PipelineLauncher.Stages
{
    internal class LambdaStage<TInput, TOutput> : Stage<TInput, TOutput>
    {
        private readonly Func<TInput, StageOption<TInput, TOutput>, Task<TOutput>> _funcAsyncWithStageOption = null;
        private readonly Func<TInput, StageOption<TInput, TOutput>, TOutput> _funcWithStageOption = null;

        private readonly StageConfiguration _configuration;

        private static readonly StageOption<TInput, TOutput> StageOption = new StageOption<TInput, TOutput>();

        private readonly Func<TInput, Task<TOutput>> _funcAsync = null;
        private readonly Func<TInput, TOutput> _func = null;

        public override StageConfiguration Configuration => _configuration ?? base.Configuration;

        internal LambdaStage(StageConfiguration stageConfiguration)
        {
            _configuration = stageConfiguration;
        }

        internal LambdaStage(Func<TInput, StageOption<TInput, TOutput>, Task<TOutput>> funcAsyncWithStageOption, StageConfiguration stageConfiguration)
            : this(stageConfiguration)
        {
            _funcAsyncWithStageOption = funcAsyncWithStageOption;
        }

        internal LambdaStage(Func<TInput, StageOption<TInput, TOutput>, TOutput> funcWithStageOption, StageConfiguration stageConfiguration)
            : this(stageConfiguration)
        {
            _funcWithStageOption = funcWithStageOption;
        }

        internal LambdaStage(Func<TInput, Task<TOutput>> funcAsync, StageConfiguration stageConfiguration)
            : this(stageConfiguration)
        {
            _funcAsync = funcAsync;
        }

        internal LambdaStage(Func<TInput, TOutput> func, StageConfiguration stageConfiguration)
            : this(stageConfiguration)
        {
            _func = func;
        }

        public override async Task<TOutput> ExecuteAsync(TInput input)
        {
            return 
                _funcAsyncWithStageOption != null ? await _funcAsyncWithStageOption(input, StageOption):
                _funcWithStageOption != null ? _funcWithStageOption(input, StageOption) :
                _funcAsync != null ? await _funcAsync(input): 
                _func(input);
        }
    }
}
