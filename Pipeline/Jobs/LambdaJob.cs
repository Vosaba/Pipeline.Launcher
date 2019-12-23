using System;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Configurations;
using PipelineLauncher.Dto;

namespace PipelineLauncher.Jobs
{
    internal class LambdaJob<TInput, TOutput> : Job<TInput, TOutput>
    {
        private readonly Func<TInput, StageOption<TInput, TOutput>, Task<TOutput>> _funcAsyncWithJobOption = null;
        private readonly Func<TInput, StageOption<TInput, TOutput>, TOutput> _funcWithJobOption = null;

        private static readonly StageOption<TInput, TOutput> StageOption = new StageOption<TInput, TOutput>();


        private readonly Func<TInput, Task<TOutput>> _funcAsync = null;
        private readonly Func<TInput, TOutput> _func = null;

        internal LambdaJob(Func<TInput, StageOption<TInput, TOutput>, Task<TOutput>> funcAsyncWithJobOption)
        {
            _funcAsyncWithJobOption = funcAsyncWithJobOption;
        }

        internal LambdaJob(Func<TInput, StageOption<TInput, TOutput>, TOutput> funcWithJobOption)
        {
            _funcWithJobOption = funcWithJobOption;
        }

        internal LambdaJob(Func<TInput, Task<TOutput>> funcAsync)
        {
            _funcAsync = funcAsync;
        }

        internal LambdaJob(Func<TInput, TOutput> func)
        {
            _func = func;
        }

        public override async Task<TOutput> ExecuteAsync(TInput input)
        {
            return 
                _funcAsyncWithJobOption != null ? await _funcAsyncWithJobOption(input, StageOption):
                _funcWithJobOption != null ? _funcWithJobOption(input, StageOption) :
                _funcAsync != null ? await _funcAsync(input): 
                _func(input);
        }
    }
}
