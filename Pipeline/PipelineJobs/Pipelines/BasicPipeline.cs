using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Collections;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Dataflow;
using PipelineLauncher.Dto;
using PipelineLauncher.Stages;

namespace PipelineLauncher.Pipelines
{
    internal class BasicPipeline<TInput, TOutput> : IPipeline<TInput, TOutput>
    {
        private readonly ITargetIn<TInput> _firstBlock;
        private readonly ITargetOut<TOutput> _lastBlock;
        private readonly CancellationToken _cancellationToken;


        internal BasicPipeline(ITargetIn<TInput> firstBlock, ITargetOut<TOutput> lastBlock)
        {
            _firstBlock = firstBlock;
            _lastBlock = lastBlock;
        }

        internal BasicPipeline(ITargetIn<TInput> firstBlock, ITargetOut<TOutput> lastBlock, CancellationToken cancellationToken)
            : this(firstBlock, lastBlock)
        {
            _cancellationToken = cancellationToken;
        }

        //TODO
        public List<Exception> Exceptions => null;

        public TOutput Run(TInput input)
        {
            return RunAsync(input).Result;
        }

        public IEnumerable<TOutput> Run(IEnumerable<TInput> input)
        {
            return RunAsync(input).Result;
        }

        public async Task<TOutput> RunAsync(TInput input)
        {
            return (await RunAsync(new[] { input })).FirstOrDefault();
        }

        public async Task<IEnumerable<TOutput>> RunAsync(IEnumerable<TInput> input)
        {
            foreach (var i in input)
            {
                _firstBlock.TryAdd(i);
            }

            _firstBlock.CompleteAdding();

            var result = new List<TOutput>();
            var consuming = new ActionBlock<TOutput>(e =>
            {
                result.Add(e);
            });

            _lastBlock.LinkTo(consuming);

            await consuming.ExecutionTask;

            return result;
        }
    }
}
