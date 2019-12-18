using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.Blocks;
using PipelineLauncher.Dto;
using PipelineLauncher.Jobs;
using PipelineLauncher.Pipelines;
using PipelineLauncher.Stage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Extensions;
using PipelineLauncher.Abstractions.Configurations;

namespace PipelineLauncher.PipelineSetup
{
    internal abstract class PipelineSetup<TFirstInput> : IPipelineSetup
    {
        protected readonly IJobService JobService;
        protected IJobService GeJobService
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

    internal partial class PipelineSetup<TInput, TOutput> : PipelineSetup<TInput>, IPipelineSetup<TInput, TOutput>//, IStageSetupOut<TOutput>, IPipelineSetup<TInput, TOutput>
    {
        public new IStageOut<TOutput> Current => (IStageOut<TOutput>)base.Current;

        internal PipelineSetup(IStageOut<TOutput> stage, IJobService jobService)
            : base(stage, jobService)
        { }

        #region Generic

        #region BulkStages

        public IPipelineSetup<TInput, TNextOutput> BulkStage<TBulkJob, TNextOutput>()
            where TBulkJob : Bulk<TOutput, TNextOutput>
            => CreateNextBulkStage<TNextOutput>(GeJobService.GetJobInstance<TBulkJob>());

        public IPipelineSetup<TInput, TOutput> BulkStage<TBulkJob>()
            where TBulkJob : Bulk<TOutput, TOutput>
            => CreateNextBulkStage<TOutput>(GeJobService.GetJobInstance<TBulkJob>());

        #endregion

        #region Stages

        public IPipelineSetup<TInput, TOutput> Stage<TJob>()
            where TJob : Job<TOutput, TOutput>
            => CreateNextStage<TOutput>(GeJobService.GetJobInstance<TJob>());

        public IPipelineSetup<TInput, TNextOutput> Stage<TJob, TNextOutput>()
            where TJob : Job<TOutput, TNextOutput>
            => CreateNextStage<TNextOutput>(GeJobService.GetJobInstance<TJob>());

        #endregion

        #endregion

        #region Nongeneric

        #region BulkStages

        public IPipelineSetup<TInput, TNextOutput> BulkStage<TNextOutput>(Bulk<TOutput, TNextOutput> bulk)
            => CreateNextBulkStage(bulk);

        public IPipelineSetup<TInput, TNextOutput> BulkStage<TNextOutput>(Func<IEnumerable<TOutput>, IEnumerable<TNextOutput>> bulkFunc, BulkJobConfiguration bulkJobConfiguration)
            => BulkStage(new LambdaBulk<TOutput, TNextOutput>(bulkFunc, bulkJobConfiguration));

        public IPipelineSetup<TInput, TNextOutput> BulkStage<TNextOutput>(Func<IEnumerable<TOutput>, Task<IEnumerable<TNextOutput>>> bulkFunc, BulkJobConfiguration bulkJobConfiguration)
            => BulkStage(new LambdaBulk<TOutput, TNextOutput>(bulkFunc, bulkJobConfiguration));

        #endregion

        #region Stages

        public IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Job<TOutput, TNextOutput> job)
            => CreateNextStage(job);

        public IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Func<TOutput, TNextOutput> func)
            => Stage(new LambdaJob<TOutput, TNextOutput>(func));

        public IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Func<TOutput, StageOption<TOutput, TNextOutput>, TNextOutput> funcWithOption)
            => Stage(new LambdaJob<TOutput, TNextOutput>(funcWithOption));

        public IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Func<TOutput, Task<TNextOutput>> func)
            => Stage(new LambdaJob<TOutput, TNextOutput>(func));

        public IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Func<TOutput, StageOption<TOutput, TNextOutput>, Task<TNextOutput>> funcWithOption)
            => Stage(new LambdaJob<TOutput, TNextOutput>(funcWithOption));

        #endregion

        #endregion

        #region Nongeneric Branch

        public IPipelineSetup<TInput, TNextOutput> Broadcast<TNextOutput>(params (Predicate<TOutput> predicate,
            Func<IPipelineSetup<TInput, TOutput>, IPipelineSetup<TInput, TNextOutput>> branch)[] branches)
        {
            BroadcastBlock<PipelineItem<TOutput>> MakeNextBlock()
            {
                var broadcastBlock = new BroadcastBlock<PipelineItem<TOutput>>(x => x);

                Current.ExecutionBlock.LinkTo(broadcastBlock);
                Current.ExecutionBlock.Completion.ContinueWith(x =>
                {
                    broadcastBlock.Complete();
                }, Current.CancellationToken);


                return broadcastBlock;
            }

            var newCurrent = CreateNextBlock(MakeNextBlock, Current.PipelineBaseConfiguration);
            return newCurrent.Branch(branches).RemoveDuplicatesPermanent();
        }

        public IPipelineSetup<TInput, TNextOutput> Branch<TNextOutput>((Predicate<TOutput> predicate,
            Func<IPipelineSetup<TInput, TOutput>, IPipelineSetup<TInput, TNextOutput>> branch)[] branches)
        {
            return Branch(ConditionExceptionScenario.GoToNextCondition, branches);
        }

        public IPipelineSetup<TInput, TNextOutput> Branch<TNextOutput>(ConditionExceptionScenario conditionExceptionScenario, (Predicate<TOutput> predicate, Func<IPipelineSetup<TInput, TOutput>, IPipelineSetup<TInput, TNextOutput>> branch)[] branches)
        {
            IPropagatorBlock<PipelineItem<TNextOutput>, PipelineItem<TNextOutput>> MakeNextBlock()
            {
                var mergeBlock = new TransformBlock<PipelineItem<TNextOutput>, PipelineItem<TNextOutput>>(x => x);

                IDataflowBlock[] headBranches = new IDataflowBlock[branches.Length];
                IDataflowBlock[] tailBranches = new IDataflowBlock[branches.Length];

                var branchId = 0;
                foreach (var branch in branches)
                {
                    var newBranchHead = new TransformBlock<PipelineItem<TOutput>, PipelineItem<TOutput>>(x => x);

                    headBranches[branchId] = newBranchHead;

                    var newtBranchStageHead = new StageOut<TOutput>(() => newBranchHead, Current.CancellationToken)
                    {
                        Previous = Current
                    };

                    Current.Next.Add(newtBranchStageHead);

                    var nextBlockAfterCurrent = new PipelineSetup<TInput, TOutput>(newtBranchStageHead, JobService);
                    var newBranch = branch.branch(nextBlockAfterCurrent);

                    Current.ExecutionBlock.LinkTo(newBranchHead,
                        x =>
                        {
                            try
                            {
                                return branch.predicate(x.Item);
                            }
                            catch (Exception ex)
                            {
                                switch (conditionExceptionScenario)
                                {
                                    case ConditionExceptionScenario.GoToNextCondition:
                                        return false;
                                    case ConditionExceptionScenario.AddExceptionAndGoToNextCondition:
                                        mergeBlock.Post(new ExceptionItem<TNextOutput>(ex, null, branch.predicate.GetType(), x.Item));
                                        return false;
                                    case ConditionExceptionScenario.BreakPipelineExecution:
                                    default:
                                        throw;
                                }
                            }
                        });

                    tailBranches[branchId] = newBranch.Current.ExecutionBlock;

                    newBranch.Current.ExecutionBlock.LinkTo(mergeBlock); //TODO broadcast TEST 

                    newBranch.Current.ExecutionBlock.Completion.ContinueWith(x =>
                    {
                        if (tailBranches.All(tail => tail.Completion.IsCompleted))
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

        public IAwaitablePipelineRunner<TInput, TOutput> CreateAwaitable(AwaitablePipelineConfig pipelineConfig = null)
        {
            var firstStage = this.GetFirstStage<TInput>();
            return new AwaitablePipelineRunner<TInput, TOutput>(() => firstStage.ExecutionBlock, () => Current.ExecutionBlock, Current.CancellationToken, () => firstStage.DestroyStageBlocks(), pipelineConfig);
        }

        public IPipelineRunner<TInput, TOutput> Create()
        {
            var firstStage = this.GetFirstStage<TInput>();
            return new PipelineRunner<TInput, TOutput>(firstStage.ExecutionBlock, Current.ExecutionBlock, Current.CancellationToken);
        }

        private PipelineSetup<TInput, TNextOutput> CreateNextStage<TNextOutput>(IPipelineJob<TOutput, TNextOutput> job)
        {
            IPropagatorBlock<PipelineItem<TOutput>, PipelineItem<TNextOutput>> MakeNextBlock()
            {
                TransformBlock<PipelineItem<TOutput>, PipelineItem<TNextOutput>> rePostBlock = null;

                void RePostMessage(PipelineItem<TOutput> message)
                {
                    rePostBlock?.Post(message);
                }

                var nextBlock = new TransformBlock<PipelineItem<TOutput>, PipelineItem<TNextOutput>>(
                    async x => await job.InternalExecute(x, () => RePostMessage(x), Current.CancellationToken),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = job.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = job.Configuration.MaxMessagesPerTask,
                        CancellationToken = Current.CancellationToken
                    });

                rePostBlock = nextBlock;

                Current.ExecutionBlock.LinkTo(nextBlock, new DataflowLinkOptions() { PropagateCompletion = true });
                Current.ExecutionBlock.Completion.ContinueWith(x =>
                {
                    nextBlock.Complete();
                }, Current.CancellationToken);

                return nextBlock;
            }

            return CreateNextBlock(MakeNextBlock, job.Configuration);
        }

        private PipelineSetup<TInput, TNextOutput> CreateNextBulkStage<TNextOutput>(IPipelineBulkJob<TOutput, TNextOutput> job)
        {
            IPropagatorBlock<PipelineItem<TOutput>, PipelineItem<TNextOutput>> MakeNextBlock()
            {
                var buffer = new BatchBlockEx<PipelineItem<TOutput>>(job.Configuration.BatchItemsCount, job.Configuration.BatchItemsTimeOut); //TODO

                TransformManyBlock<IEnumerable<PipelineItem<TOutput>>, PipelineItem<TNextOutput>> rePostBlock = null;

                void RePostMessages(IEnumerable<PipelineItem<TOutput>> messages)
                {
                    rePostBlock?.Post(messages);
                }

                var nextBlock = new TransformManyBlock<IEnumerable<PipelineItem<TOutput>>, PipelineItem<TNextOutput>>(
                    async x => await job.InternalExecute(x, () => RePostMessages(x), Current.CancellationToken),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = job.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = job.Configuration.MaxMessagesPerTask,
                        CancellationToken = Current.CancellationToken
                    });

                buffer.LinkTo(nextBlock, new DataflowLinkOptions() { PropagateCompletion = true });
                rePostBlock = nextBlock;

                buffer.Completion.ContinueWith(x =>
                {
                    nextBlock.Complete();
                }, Current.CancellationToken);

                var next = DataflowBlock.Encapsulate(buffer, nextBlock);

                Current.ExecutionBlock.LinkTo(next, new DataflowLinkOptions() { PropagateCompletion = true });

                Current.ExecutionBlock.Completion.ContinueWith(x =>
                {
                    next.Complete();
                }, Current.CancellationToken);

                return next;
            }

            return CreateNextBlock(MakeNextBlock, job.Configuration);
        }

        private PipelineSetup<TInput, TNextOutput> CreateNextBlock<TNextOutput>(Func<IPropagatorBlock<PipelineItem<TOutput>, PipelineItem<TNextOutput>>> nextBlock, PipelineBaseConfiguration pipelineBaseConfiguration)
        {
            var nextStage = new StageOut<TNextOutput>(nextBlock, Current.CancellationToken)
            {
                Previous = Current,
                PipelineBaseConfiguration = pipelineBaseConfiguration
            };

            Current.Next.Add(nextStage);

            return new PipelineSetup<TInput, TNextOutput>(nextStage, JobService);
        }
    }
}