using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Collections;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.Pipeline;
//using PipelineLauncher.Dataflow;
using PipelineLauncher.Dto;
using PipelineLauncher.Stages;

namespace PipelineLauncher.Pipelines
{
    internal class BasicPipeline<TInput, TOutput> : IPipeline<TInput, TOutput>
    {
        private readonly ITargetBlock<PipelineItem<TInput>> _firstBlock;
        private readonly ISourceBlock<PipelineItem<TOutput>> _lastBlock;
        private readonly CancellationToken _cancellationToken;


        internal BasicPipeline(ITargetBlock<PipelineItem<TInput>> firstBlock, ISourceBlock<PipelineItem<TOutput>> lastBlock)
        {
            _firstBlock = firstBlock;
            _lastBlock = lastBlock;
        }

        internal BasicPipeline(ITargetBlock<PipelineItem<TInput>> firstBlock, ISourceBlock<PipelineItem<TOutput>> lastBlock, CancellationToken cancellationToken)
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

        private bool isLinked = false;
        private static List<TOutput> result = new List<TOutput>();

        private ActionBlock<PipelineItem<TOutput>> consuming = new ActionBlock<PipelineItem<TOutput>>(e =>
        {
            result.Add(e.Item);
        });


        public async Task<IEnumerable<TOutput>> RunAsync(IEnumerable<TInput> input)
        {
            //if(_firstBlock.Completion.IsCompleted)
            //    _firstBlock.Completion.Start();

            foreach (var i in input)
            {
                _firstBlock.Post(new PipelineItem<TInput>(i));
            }

            //_firstBlock.Complete();

            if (!isLinked)
            {
                _lastBlock.LinkTo(consuming, new DataflowLinkOptions() { PropagateCompletion = false });

                _lastBlock.Completion.ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        //((IDataflowBlock)_step2A).Fault(t.Exception);
                        //((IDataflowBlock)_step2B).Fault(t.Exception);
                    }
                    else
                    {
                        //consuming.Complete();
                    }
                });

                isLinked = true;
            }



            //_lastBlock.Completion.Wait();
            //await consuming.ExecutionTask;
            //consuming.Completion.Wait();

           return result;
        }
    }
}
