using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Collections;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Dto;

namespace PipelineLauncher.Pipelines
{
    internal class BasicPipeline<TInput, TOutput> : IPipeline<TInput, TOutput>
    {
        private readonly IEnumerable<IPipelineJob> _jobs;
        private readonly CancellationToken _cancellationToken;

        internal BasicPipeline(IEnumerable<IPipelineJob> jobs)
        {
            _jobs = jobs;
        }

        internal BasicPipeline(IEnumerable<IPipelineJob> jobs, CancellationToken cancellationToken)
            : this(jobs)
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
            var tasks = new HashSet<Task>();
            var stages = Inline(input.ToQueue(_cancellationToken), _jobs.ToArray()).ToArray();

            foreach (var stage in stages)
            {
                tasks.Add(Task.Run(() => ExecuteStage(stage.Key, stage.Value), _cancellationToken));
            }

            await Task.WhenAll(tasks.ToArray());
            return stages.Last().Value.Output.GetElements(_cancellationToken)
                .Where(e => !(e is StageSkipObject)).Cast<TOutput>().ToList();
        }

        private IEnumerable<KeyValuePair<IQueue<object>, IPipelineJob>> Inline(IQueue<object> firstQueue, IReadOnlyList<IPipelineJob> jobs)
        {
            if(!jobs.Any())
                yield break;

            var lastJob = jobs[0];
            yield return new KeyValuePair<IQueue<object>, IPipelineJob>(firstQueue, lastJob);

            foreach (var pipelineJob in jobs.Skip(1))
            {
                lastJob.InitOutput();
                yield return new KeyValuePair<IQueue<object>, IPipelineJob>(lastJob.Output, pipelineJob);
                lastJob = pipelineJob;
            }
        }

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
                                        asyncJob.InternalPerform(skipFilter.Item, _cancellationToken);
                                        break;
                                    case StageSkipObject _:
                                        asyncJob.Output.Add(item, _cancellationToken);
                                        break;
                                    default:
                                        asyncJob.InternalPerform(item, _cancellationToken);
                                        break;
                                }
                            });
                        break;
                    case IPipelineJobSync syncJob:

                        var fullInput = input.GetElements(_cancellationToken).ToArray();

                        syncJob.InternalPerform(fullInput
                                .Where(e => !(e is StageSkipObject skip) || skip.CanProcess(syncJob))
                                .Select(e => e is StageSkipObject skip ? skip.Item : e).ToArray(), _cancellationToken);

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
