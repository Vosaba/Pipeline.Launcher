using System;
using System.Threading.Tasks;
using PipelineLauncher.Dto;

namespace PipelineLauncher.Jobs
{
    internal class AsyncLambdaJob<TInput, TOutput> : AsyncJob<TInput, TOutput>
    {
        private readonly Func<TInput, AsyncJobOption<TInput, TOutput>, Task<TOutput>> _funcAsyncWithJobOption = null;
        private readonly Func<TInput, AsyncJobOption<TInput, TOutput>, TOutput> _funcWithJobOption = null;


        private readonly Func<TInput, Task<TOutput>> _funcAsync = null;
        private readonly Func<TInput, TOutput> _func = null;

        internal AsyncLambdaJob(Func<TInput, AsyncJobOption<TInput, TOutput>, Task<TOutput>> funcAsyncWithJobOption)
        {
            _funcAsyncWithJobOption = funcAsyncWithJobOption;
        }

        internal AsyncLambdaJob(Func<TInput, AsyncJobOption<TInput, TOutput>, TOutput> funcWithJobOption)
        {
            _funcWithJobOption = funcWithJobOption;
        }

        internal AsyncLambdaJob(Func<TInput, Task<TOutput>> funcAsync)
        {
            _funcAsync = funcAsync;
        }

        internal AsyncLambdaJob(Func<TInput, TOutput> func)
        {
            _func = func;
        }

        public override async Task<TOutput> ExecuteAsync(TInput input)
        {
            return 
                _funcAsyncWithJobOption != null ? await _funcAsyncWithJobOption(input, AsyncJobOption):
                _funcWithJobOption != null ? _funcWithJobOption(input, AsyncJobOption) :
                _funcAsync != null ? await _funcAsync(input): 
                _func(input);
        }
    }
}
