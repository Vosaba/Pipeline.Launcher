using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Dto;

namespace PipelineLauncher.Stages
{
    internal class LambdaBulkStage<TInput, TOutput> : BulkStage<TInput, TOutput>
    {
        private readonly Func<IEnumerable<TInput>, Task<IEnumerable<TOutput>>> _funcAsync;
        private readonly Func<IEnumerable<TInput>, IEnumerable<TOutput>> _func;
        private readonly BulkStageConfiguration _configuration;
        public override BulkStageConfiguration Configuration => _configuration != null? _configuration: base.Configuration;

        private LambdaBulkStage(BulkStageConfiguration bulkStageConfiguration)
        {
           _configuration = bulkStageConfiguration;
        }

        public LambdaBulkStage(Func<IEnumerable<TInput>, Task<IEnumerable<TOutput>>> funcAsync, BulkStageConfiguration bulkStageConfiguration)
            : this(bulkStageConfiguration)
        {
            _funcAsync = funcAsync;
        }

        public LambdaBulkStage(Func<IEnumerable<TInput>, IEnumerable<TOutput>> func, BulkStageConfiguration bulkStageConfiguration)
            : this(bulkStageConfiguration)
        {
            _func = func;
        }

        public override async Task<IEnumerable<TOutput>> ExecuteAsync(IEnumerable<TInput> input)
        {
            return _funcAsync != null ? await _funcAsync(input) : _func(input);
        }
    }
}