using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.Stage
{
    internal class Stage : IStage
    {
        private IDataflowBlock _executionBlock;

        public Func<IDataflowBlock> CreateBlock { get; }
        public CancellationToken CancellationToken { get; set; }
        public IList<IStage> Next { get; set; } = new List<IStage>();
        public IStage Previous { get; set; }
        public IDataflowBlock ExecutionBlock
        {
            get => _executionBlock ??= CreateBlock();
            set => _executionBlock = value;
        }

        public Stage(Func<IDataflowBlock> createTerra, CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
            CreateBlock = createTerra;
        }

        public void DestroyBlock()
        {
            ExecutionBlock = null;
        }
    }

    internal class Stage<TIn, TOut> : StageOut<TOut>, IStage<TIn, TOut>
    {
        public Stage(Func<IPropagatorBlock<PipelineItem<TIn>, PipelineItem<TOut>>> createTerra,  CancellationToken cancellationToken)
            : base(createTerra, cancellationToken)
        { }

        ITargetBlock<PipelineItem<TIn>> IStageIn<TIn>.ExecutionBlock => (ITargetBlock<PipelineItem<TIn>>)ExecutionBlock;
        IPropagatorBlock<PipelineItem<TIn>, PipelineItem<TOut>> IStage<TIn, TOut>.ExecutionBlock => (IPropagatorBlock<PipelineItem<TIn>, PipelineItem<TOut>>)ExecutionBlock;
    }
}
