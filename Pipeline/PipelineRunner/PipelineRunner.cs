using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineRunner;
using PipelineLauncher.Abstractions.PipelineRunner.Configurations;
using PipelineLauncher.Abstractions.PipelineStage;
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

        public bool Post(TInput input)
        {
            return Post(new [] {input});
        }

        public bool Post(IEnumerable<TInput> input)
        {
            return input.Select(x => new PipelineStageItem<TInput>(x)).All(x => _firstBlock.Post(x));
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
