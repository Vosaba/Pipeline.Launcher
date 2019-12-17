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
        private readonly BulkJobConfiguration _configuration;
        public override BulkJobConfiguration Configuration => _configuration != null? _configuration: base.Configuration;

        private LambdaBulkJob(BulkJobConfiguration bulkJobConfiguration)
        {
           _configuration = bulkJobConfiguration;
        }

        public LambdaBulkJob(Func<IEnumerable<TInput>, Task<IEnumerable<TOutput>>> funcAsync, BulkJobConfiguration bulkJobConfiguration)
            : this(bulkJobConfiguration)
        {
            _funcAsync = funcAsync;
        }

        public LambdaBulkJob(Func<IEnumerable<TInput>, IEnumerable<TOutput>> func, BulkJobConfiguration bulkJobConfiguration)
            : this(bulkJobConfiguration)
        {
            _func = func;
        }

        public override async Task<IEnumerable<TOutput>> ExecuteAsync(IEnumerable<TInput> input)
        {
            return _funcAsync != null ? await _funcAsync(input) : _func(input);
        }
    }
}