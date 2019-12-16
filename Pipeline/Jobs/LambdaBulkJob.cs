using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Configurations;

namespace PipelineLauncher.Jobs
{
    internal class LambdaBulkJob<TInput, TOutput> : BulkJob<TInput, TOutput>
    {
        private readonly Func<IEnumerable<TInput>, Task<IEnumerable<TOutput>>> _funcAsync;
        private readonly Func<IEnumerable<TInput>, IEnumerable<TOutput>> _func;


        public LambdaBulkJob(Func<IEnumerable<TInput>, Task<IEnumerable<TOutput>>> funcAsync)
        {
            _funcAsync = funcAsync;
        }

        public LambdaBulkJob(Func<IEnumerable<TInput>, IEnumerable<TOutput>> func)
        {
            _func = func;
        }

        public override async Task<IEnumerable<TOutput>> ExecuteAsync(IEnumerable<TInput> input)
        {
            return _funcAsync != null ? await _funcAsync(input) : _func(input);
        }
    }
}