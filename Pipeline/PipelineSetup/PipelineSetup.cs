using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.Blocks;
using PipelineLauncher.Dto;
using PipelineLauncher.Extensions;
using PipelineLauncher.Jobs;
using PipelineLauncher.Pipelines;
using PipelineLauncher.Stage;

namespace PipelineLauncher.PipelineSetup
{
    internal abstract class PipelineSetup<TFirstInput> : IPipelineSetup
    {
        protected readonly IJobService JobService;
        protected IJobService GetJobService
        {
            get
            {
                if (JobService == null)
                {
                    throw new Exception($"'{nameof(IJobService)}' isn't provided, if you need to use Generic stage setups, provide service.");
                }

                return JobService;
            }
        }

        public IStage Current { get; }

        internal PipelineSetup(IStage stage, IJobService jobService)
        {
            Current = stage;
            JobService = jobService;
        }
    }

    internal class PipelineSetup<TInput, TOutput> : PipelineSetup<TInput>, IStageSetupOut<TOutput> , IPipelineSetup<TInput, TOutput>
    {
        public new IStageOut<TOutput> Current => (IStageOut<TOutput>)base.Current;

        internal PipelineSetup(IStageOut<TOutput> stage, IJobService jobService)
            : base(stage, jobService)
        { }

        #region Generic Stages

        public IPipelineSetup<TInput, TNextOutput> Stage<TJob, TNextOutput>()
            where TJob : Job<TOutput, TNextOutput>
            => CreateNextStage<TNextOutput>(GetJobService.GetJobInstance<TJob>());

        public IPipelineSetup<TInput, TOutput> Stage<TJob>()
            where TJob : Job<TOutput, TOutput>
            => CreateNextStage<TOutput>(GetJobService.GetJobInstance<TJob>());

        public IPipelineSetup<TInput, TNextOutput> Stage<TJob, TJob2, TNextOutput>()
            where TJob : Job<TOutput, TNextOutput>
            where TJob2 : Job<TOutput, TNextOutput>
            => Stage(GetJobService.GetJobInstance<TJob>(), GetJobService.GetJobInstance<TJob2>());

        public IPipelineSetup<TInput, TNextOutput> Stage<TJob, TJob2, TJob3, TNextOutput>()
            where TJob : Job<TOutput, TNextOutput>
            where TJob2 : Job<TOutput, TNextOutput>
            where TJob3 : Job<TOutput, TNextOutput>
            => Stage(GetJobService.GetJobInstance<TJob>(), GetJobService.GetJobInstance<TJob2>(), GetJobService.GetJobInstance<TJob3>());

        public IPipelineSetup<TInput, TNextOutput> Stage<TJob, TJob2, TJob3, TJob4, TNextOutput>()
            where TJob : Job<TOutput, TNextOutput>
            where TJob2 : Job<TOutput, TNextOutput>
            where TJob3 : Job<TOutput, TNextOutput>
            where TJob4 : Job<TOutput, TNextOutput>
            => Stage(GetJobService.GetJobInstance<TJob>(), GetJobService.GetJobInstance<TJob2>(), GetJobService.GetJobInstance<TJob3>(), GetJobService.GetJobInstance<TJob4>());

        public IPipelineSetup<TInput, TOutput> AsyncStage<TAsyncJob>()
            where TAsyncJob : AsyncJob<TOutput, TOutput>
            => CreateNextStageAsync<TOutput>(GetJobService.GetJobInstance<TAsyncJob>());

        public IPipelineSetup<TInput, TNextOutput> AsyncStage<TAsyncJob, TNextOutput>()
            where TAsyncJob : AsyncJob<TOutput, TNextOutput>
            => CreateNextStageAsync<TNextOutput>(GetJobService.GetJobInstance<TAsyncJob>());

        public IPipelineSetup<TInput, TNextOutput> AsyncStage<TAsyncJob, TAsyncJob2, TNextOutput>()
            where TAsyncJob : AsyncJob<TOutput, TNextOutput>
            where TAsyncJob2 : AsyncJob<TOutput, TNextOutput>
            => AsyncStage(GetJobService.GetJobInstance<TAsyncJob>(), GetJobService.GetJobInstance<TAsyncJob2>());

        public IPipelineSetup<TInput, TNextOutput> AsyncStage<TAsyncJob, TAsyncJob2, TAsyncJob3, TNextOutput>()
            where TAsyncJob : AsyncJob<TOutput, TNextOutput>
            where TAsyncJob2 : AsyncJob<TOutput, TNextOutput>
            where TAsyncJob3 : AsyncJob<TOutput, TNextOutput>
            => AsyncStage(GetJobService.GetJobInstance<TAsyncJob>(), GetJobService.GetJobInstance<TAsyncJob2>(), GetJobService.GetJobInstance<TAsyncJob3>());

        public IPipelineSetup<TInput, TNextOutput> AsyncStage<TAsyncJob, TAsyncJob2, TAsyncJob3, TAsyncJob4, TNextOutput>()
            where TAsyncJob : AsyncJob<TOutput, TNextOutput>
            where TAsyncJob2 : AsyncJob<TOutput, TNextOutput>
            where TAsyncJob3 : AsyncJob<TOutput, TNextOutput>
            where TAsyncJob4 : AsyncJob<TOutput, TNextOutput>
            => AsyncStage(GetJobService.GetJobInstance<TAsyncJob>(), GetJobService.GetJobInstance<TAsyncJob2>(), GetJobService.GetJobInstance<TAsyncJob3>(), GetJobService.GetJobInstance<TAsyncJob4>());

        #endregion

        #region Nongeneric Stages

        public IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Job<TOutput, TNextOutput> job)
            => CreateNextStage(job);

        public IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Func<IEnumerable<TOutput>, IEnumerable<TNextOutput>> func)
            => Stage(new LambdaJob<TOutput, TNextOutput>(async x => await Task.FromResult(func(x))));

        public IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Func<IEnumerable<TOutput>, Task<IEnumerable<TNextOutput>>> func)
            => Stage(new LambdaJob<TOutput, TNextOutput>(func));

        public IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(params Job<TOutput, TNextOutput>[] jobs)
            => Stage(new ConditionJob<TOutput, TNextOutput>(jobs));

        public IPipelineSetup<TInput, TNextOutput> AsyncStage<TNextOutput>(AsyncJob<TOutput, TNextOutput> asyncJob)
            => CreateNextStageAsync(asyncJob);

        public IPipelineSetup<TInput, TNextOutput> AsyncStage<TNextOutput>(Func<TOutput, TNextOutput> asyncFunc)
            => AsyncStage(new AsyncLambdaJob<TOutput, TNextOutput>(asyncFunc));

        public IPipelineSetup<TInput, TNextOutput> AsyncStage<TNextOutput>(Func<TOutput, AsyncJobOption<TOutput, TNextOutput>, TNextOutput> asyncFuncWithOption)
            => AsyncStage(new AsyncLambdaJob<TOutput, TNextOutput>(asyncFuncWithOption));

        public IPipelineSetup<TInput, TNextOutput> AsyncStage<TNextOutput>(Func<TOutput, Task<TNextOutput>> asyncFunc)
            => AsyncStage(new AsyncLambdaJob<TOutput, TNextOutput>(asyncFunc));

        public IPipelineSetup<TInput, TNextOutput> AsyncStage<TNextOutput>(Func<TOutput, AsyncJobOption<TOutput, TNextOutput>, Task<TNextOutput>> asyncFuncWithOption)
            => AsyncStage(new AsyncLambdaJob<TOutput, TNextOutput>(asyncFuncWithOption));


        public IPipelineSetup<TInput, TNextOutput> AsyncStage<TNextOutput>(params AsyncJob<TOutput, TNextOutput>[] asyncJobs)
            => AsyncStage(new ConditionAsyncJob<TOutput, TNextOutput>(asyncJobs));

        #endregion

        #region Nongeneric Branch

        public IPipelineSetup<TInput, TNextOutput> Broadcast<TNextOutput>(params (Predicate<TOutput> predicate,
            Func<IPipelineSetup<TInput, TOutput>, IPipelineSetup<TInput, TNextOutput>> branch)[] branches)
        {
            BroadcastBlock<PipelineItem<TOutput>> MakeNextBlock()
            {
                var broadcastBlock =  new BroadcastBlock<PipelineItem<TOutput>>(e => e);

                Current.ExecutionBlock.LinkTo(broadcastBlock);
                Current.ExecutionBlock.Completion.ContinueWith(x =>
                {
                    broadcastBlock.Complete();
                }, Current.CancellationToken);


                return broadcastBlock;
            }

            var newCurrent = CreateNextBlock(MakeNextBlock);
            return newCurrent.Branch(branches).RemoveDuplicates();
        }

        public IPipelineSetup<TInput, TNextOutput> Branch<TNextOutput>(params (Predicate<TOutput> predicate,
            Func<IPipelineSetup<TInput, TOutput>, IPipelineSetup<TInput, TNextOutput>> branch)[] branches)
        {
            IPropagatorBlock<PipelineItem<TNextOutput>, PipelineItem<TNextOutput>> MakeNextBlock()
            {
                var mergeBlock = new TransformBlock<PipelineItem<TNextOutput>, PipelineItem<TNextOutput>>(e => e);

                IDataflowBlock[] headBranches = new IDataflowBlock[branches.Length];
                IDataflowBlock[] tailBranches = new IDataflowBlock[branches.Length];

                var branchId = 0;
                foreach (var branch in branches)
                {
                    var newBranchHead = new TransformBlock<PipelineItem<TOutput>, PipelineItem<TOutput>>(e => e);

                    headBranches[branchId] = newBranchHead;

                    var newtBranchStageHead = new StageOut<TOutput>(() => newBranchHead, Current.CancellationToken)
                    {
                        Previous = Current
                    };

                    Current.Next.Add(newtBranchStageHead);

                    var nextBlockAfterCurrent = new PipelineSetup<TInput, TOutput>(newtBranchStageHead, JobService);
                    var newBranch = branch.branch(nextBlockAfterCurrent);

                    Current.ExecutionBlock.LinkTo(newBranchHead, e => branch.predicate(e.Item)); //TODO AAAAA ctach

                    tailBranches[branchId] = newBranch.Current.ExecutionBlock;

                    newBranch.Current.ExecutionBlock.LinkTo(mergeBlock); //TODO broadcast TEST 

                    newBranch.Current.ExecutionBlock.Completion.ContinueWith(x =>
                    {
                        if (tailBranches.All(e => e.Completion.IsCompleted))
                        {
                            mergeBlock.Complete();
                        }
                    }, Current.CancellationToken);

                    branchId++;
                }

                Current.ExecutionBlock.Completion.ContinueWith(x =>
                {
                    foreach (var headBranch in headBranches)
                    {
                        headBranch.Complete();
                    }
                }, Current.CancellationToken);

                return mergeBlock;
            };

            var t = new StageOut<TNextOutput>(MakeNextBlock, Current.CancellationToken)
            {
                Previous = Current
            };

            Current.Next.Add(t); // Hack with cross linking to destroy

            return new PipelineSetup<TInput, TNextOutput>(t, JobService);
        }

        #endregion

        public IAwaitablePipelineRunner<TInput, TOutput> CreateAwaitable()
        {
            var firstStage = this.GetFirstStage<TInput>();
            return new AwaitablePipelineRunner<TInput, TOutput>(()=>firstStage.ExecutionBlock, ()=>Current.ExecutionBlock, Current.CancellationToken, () => DestroyStageBlocks(firstStage));
        }

        public IPipelineRunner<TInput, TOutput> Create()
        {
            var firstStage = this.GetFirstStage<TInput>();
            return new PipelineRunner<TInput, TOutput>(firstStage.ExecutionBlock, Current.ExecutionBlock, Current.CancellationToken);
        }

        private PipelineSetup<TInput, TNextOutput> CreateNextStageAsync<TNextOutput>(IPipelineJobAsync<TOutput, TNextOutput> asyncJob)
        {
            IPropagatorBlock<PipelineItem<TOutput>, PipelineItem<TNextOutput>> MakeNextBlock()
            {
                TransformBlock<PipelineItem<TOutput>, PipelineItem<TNextOutput>> rePostBlock = null;

                void RePostMessage(PipelineItem<TOutput> message)
                {
                    rePostBlock?.Post(message);
                }

                var nextBlock = new TransformBlock<PipelineItem<TOutput>, PipelineItem<TNextOutput>>(
                    async e => await asyncJob.InternalExecute(e, () => RePostMessage(e), Current.CancellationToken),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = asyncJob.MaxDegreeOfParallelism
                        ,
                        MaxMessagesPerTask = 1
                    });

                rePostBlock = nextBlock;

                //var next = DataflowBlock.Encapsulate(buffer, nextBlock);

                Current.ExecutionBlock.LinkTo(nextBlock, new DataflowLinkOptions() { PropagateCompletion = true });

                Current.ExecutionBlock.Completion.ContinueWith(x =>
                {
                    nextBlock.Complete();
                }, Current.CancellationToken);

                return nextBlock;
            }

            return CreateNextBlock(MakeNextBlock);
        }

        private PipelineSetup<TInput, TNextOutput> CreateNextStage<TNextOutput>(IPipelineJobSync<TOutput, TNextOutput> job)
        {
            IPropagatorBlock<PipelineItem<TOutput>, PipelineItem<TNextOutput>> MakeNextBlock()
            {
                var buffer = new BatchBlockEx<PipelineItem<TOutput>>(20, 5000); //TODO

                TransformManyBlock<IEnumerable<PipelineItem<TOutput>>, PipelineItem<TNextOutput>> rePostBlock = null;

                void RePostMessages(IEnumerable<PipelineItem<TOutput>> messages)
                {
                    rePostBlock?.Post(messages);
                }

                var nextBlock = new TransformManyBlock<IEnumerable<PipelineItem<TOutput>>, PipelineItem<TNextOutput>>(
                    async e => await job.InternalExecute(e, () => RePostMessages(e), Current.CancellationToken),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = job.MaxDegreeOfParallelism
                        ,MaxMessagesPerTask = 1
                    });

                buffer.LinkTo(nextBlock, new DataflowLinkOptions() { PropagateCompletion = true });
                rePostBlock = nextBlock;

                buffer.Completion.ContinueWith(x => { nextBlock.Complete(); }, Current.CancellationToken);

                var next = DataflowBlock.Encapsulate(buffer, nextBlock);

                Current.ExecutionBlock.LinkTo(next, new DataflowLinkOptions() { PropagateCompletion = true });

                Current.ExecutionBlock.Completion.ContinueWith(x =>
                {
                    next.Complete();
                }, Current.CancellationToken);

                return next;
            }

            return CreateNextBlock(MakeNextBlock);
        }

        private PipelineSetup<TInput, TNextOutput> CreateNextBlock<TNextOutput>(Func<IPropagatorBlock<PipelineItem<TOutput>, PipelineItem<TNextOutput>>> nextBlock)
        {
            var nextStage = new StageOut<TNextOutput>(nextBlock, Current.CancellationToken)
            {
                Previous = Current
            };

            Current.Next.Add(nextStage);

            return new PipelineSetup<TInput, TNextOutput>(nextStage, JobService);
        }

        private static void DestroyStageBlocks(IStage stage)
        {
            if (stage == null)
            {
                return;
            }

            stage.DestroyBlock();

            if (stage.Next == null || !stage.Next.Any()) return;
            foreach (var nextStage in stage.Next)
            {
                DestroyStageBlocks(nextStage);
            }
        }
    }
}