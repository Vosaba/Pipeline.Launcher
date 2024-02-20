using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipelineLauncher.Stages
{
    internal class LambdaBulkStage<TInput, TOutput> : BulkStage<TInput, TOutput>
    {
        private readonly Func<TInput[], Task<IEnumerable<TOutput>>> _funcAsync;
        private readonly Func<TInput[], IEnumerable<TOutput>> _func;
        private readonly BulkStageConfiguration _configuration;

        public override BulkStageConfiguration Configuration => _configuration ?? base.Configuration;

        private LambdaBulkStage(BulkStageConfiguration bulkStageConfiguration)
        {
           _configuration = bulkStageConfiguration;
        }

        public LambdaBulkStage(Func<TInput[], Task<IEnumerable<TOutput>>> funcAsync, BulkStageConfiguration bulkStageConfiguration)
            : this(bulkStageConfiguration)
        {
            _funcAsync = funcAsync;
        }

        public LambdaBulkStage(Func<TInput[], IEnumerable<TOutput>> func, BulkStageConfiguration bulkStageConfiguration)
            : this(bulkStageConfiguration)
        {
            _func = func;
        }

        public override async Task<IEnumerable<TOutput>> ExecuteAsync(TInput[] input)
        {
            return _funcAsync != null ? await _funcAsync(input) : _func(input);
        }
    }
}