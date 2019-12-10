using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipelineLauncher.Jobs
{
    internal class LambdaJob<TInput, TOutput> : Job<TInput, TOutput>
    {
        private readonly Func<IEnumerable<TInput>, Task<IEnumerable<TOutput>>> _func;

        public LambdaJob(Func<IEnumerable<TInput>, Task<IEnumerable<TOutput>>> func)
        {
            _func = func;
        }

        public override Task<IEnumerable<TOutput>> ExecuteAsync(IEnumerable<TInput> input)
        {
            return _func(input);
        }
    }
}