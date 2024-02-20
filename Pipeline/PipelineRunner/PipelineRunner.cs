using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineRunner;
using PipelineLauncher.Abstractions.PipelineRunner.Configurations;
using PipelineLauncher.PipelineStage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace PipelineLauncher.PipelineRunner
{
    internal class PipelineRunner<TInput, TOutput> : PipelineRunnerBase<TInput, TOutput>, IPipelineRunner<TInput, TOutput>
    {
        private readonly PipelineCreationConfig _pipelineCreationConfig;
        private readonly ITargetBlock<PipelineStageItem<TInput>> _firstBlock;
        private readonly ISourceBlock<PipelineStageItem<TOutput>> _lastBlock;

        internal PipelineRunner(
            Func<StageCreationContext, ITargetBlock<PipelineStageItem<TInput>>> retrieveFirstBlock,
            Func<StageCreationContext, ISourceBlock<PipelineStageItem<TOutput>>> retrieveLastBlock,
            PipelineCreationConfig pipelineCreationConfig)
            : base(retrieveFirstBlock, retrieveLastBlock, new StageCreationContext(PipelineType.Normal, true))
        {
            _pipelineCreationConfig = pipelineCreationConfig;

            _firstBlock = RetrieveFirstBlock(StageCreationContext);
            _lastBlock = RetrieveLastBlock(StageCreationContext);

            GenerateSortingBlock(_lastBlock);
        }

        public bool Post(TInput input) => _firstBlock.Post(new PipelineStageItem<TInput>(input));

        public bool Post(IEnumerable<TInput> input) => input.All(Post);

        public async Task<bool> PostAsync(TInput input) => await _firstBlock.SendAsync(new PipelineStageItem<TInput>(input));

        public async Task<bool> PostAsync(IEnumerable<TInput> input)
        {
            foreach (var item in input)
            {
                if (!await PostAsync(item))
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> PostAsync(TInput input, CancellationToken cancellationToken) => await _firstBlock.SendAsync(new PipelineStageItem<TInput>(input), cancellationToken);

        public async Task<bool> PostAsync(IEnumerable<TInput> input, CancellationToken cancellationToken)
        {
            foreach (var item in input)
            {
                if (!await PostAsync(item, cancellationToken))
                {
                    return false;
                }
            }

            return true;
        }

        public Task CompleteExecution()
        {
            _firstBlock.Complete();
            return _lastBlock.Completion;
        }

        IPipelineRunner<TInput, TOutput> IPipelineRunner<TInput, TOutput>.SetupCancellationToken(CancellationToken cancellationToken)
            => (IPipelineRunner<TInput, TOutput>)SetupCancellationToken(cancellationToken);
    }
}
