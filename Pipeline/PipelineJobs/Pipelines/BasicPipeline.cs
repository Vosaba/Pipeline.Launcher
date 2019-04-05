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

        private readonly List<IExecutionBlock> _blocks = new List<IExecutionBlock>();
        private ITargetIn<TInput> _startTask;
        public Task<TOutput> EndTask;

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

            var consuming = new ActionBlock<TOutput>((e) => result.Add(e));

            _lastBlock.LinkTo(consuming);
            await consuming.ExecutionTask;

            return result;
            //var tasks = new HashSet<Task>();
            //KeyValuePair<IEnumerable<IQueue<object>>, IPipelineJob>[] stages = Inline(_rootStage, new[]{input.ToQueue(_cancellationToken)}).ToArray();

            //foreach (var stage in stages)
            //{
            //    //tasks.Add(Task.Run(() => ExecuteStage(stage.Key, stage.Value), _cancellationToken));
            //}

            //await Task.WhenAll(tasks.ToArray());
            //return stages.Last().Value.Output.GetElements(_cancellationToken)
            //    .Where(e => !(e is StageSkipObject)).Cast<TOutput>().ToList();
        }

        //private void MakeDataFlow(IStage stage)
        //{
        //    switch (stage)
        //    {
        //       case  
        //    }
        //}

        //private IEnumerable<KeyValuePair<IEnumerable<IQueue<object>>, IPipelineJob>> Inline(IStage stage,
        //    IEnumerable<IQueue<object>> firstQueue)
        //{
        //    var result = new List<KeyValuePair<IEnumerable<IQueue<object>>, IPipelineJob>>();

        //    Inline(stage, result, firstQueue);

        //    return result;
        //}

        //private void Inline(IStage stage, IList<KeyValuePair<IEnumerable<IQueue<object>>, IPipelineJob>> result, IEnumerable<IQueue<object>> firstQueue = null)
        //{
        //    if (firstQueue != null)
        //    {
        //        result.Add(new KeyValuePair<IEnumerable<IQueue<object>>, IPipelineJob>(firstQueue, stage.Job));
        //    }
        //    else if(stage.Previous != null)
        //    {
        //        foreach (var previousStage in stage.Previous)
        //        {
        //            previousStage.Job.InitOutput();
        //        }

                
        //    }

        //    if (stage.Next != null)
        //    {
        //        foreach (IStage nextStage in stage.Next)
        //        {
        //            nextStage.Job.InitOutput();

        //            Inline(nextStage, result, nextStage.Previous.Select(e => e.Job.Output));
        //        }

        //        result.Add(new KeyValuePair<IEnumerable<IQueue<object>>, IPipelineJob>(new []{stage.Job.Output}, stage.Job));
        //    }
        //    else
        //    {
        //        //return new KeyValuePair<IEnumerable<IQueue<object>>, IPipelineJob>(stage.Previous.Select(e => e.Job.Output), stage.Job);
        //    }
        //}

        private void ExecuteStage(IQueue<object> input, IPipelineJob job)
        {
            try
            {
                switch (job)
                {
                    case IPipelineJobAsync asyncJob:

                        var parallelOptions = new ParallelOptions
                        {
                            MaxDegreeOfParallelism = asyncJob.MaxDegreeOfParallelism,
                            CancellationToken = _cancellationToken
                        };

                        Parallel.ForEach(input.GetElements(_cancellationToken), parallelOptions,
                            item =>
                            {
                                switch (item)
                                {
                                    case StageSkipObject skipFilter when skipFilter.CanProcess(asyncJob):
                                        asyncJob.InternalExecute(skipFilter.Item, _cancellationToken);
                                        break;
                                    case StageSkipObject _:
                                        asyncJob.Output.Add(item, _cancellationToken);
                                        break;
                                    default:
                                        asyncJob.InternalExecute(item, _cancellationToken);
                                        break;
                                }
                            });
                        break;
                    case IPipelineJobSync syncJob:

                        var fullInput = input.GetElements(_cancellationToken).ToArray();

                        syncJob.InternalExecute(fullInput
                                .Where(e => !(e is StageSkipObject skip) || skip.CanProcess(syncJob))
                                .Select(e => e is StageSkipObject skip ? skip.Item : e).ToArray(), _cancellationToken).ToArray();

                        foreach (var item in fullInput.Where(e => e is StageSkipObject))
                        {
                            job.Output.Add(item, _cancellationToken);
                        }
                        break;
                }
            }
            catch (OperationCanceledException){}
            finally
            {
                job.Output.CompleteAdding();
            }
        }
    }
}
