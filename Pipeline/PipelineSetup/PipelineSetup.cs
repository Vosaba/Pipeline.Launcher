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
using PipelineLauncher.Abstractions.PipelineEvents;

namespace PipelineLauncher.PipelineSetup
{
    internal abstract class PipelineSetup<TFirstInput> : IPipelineSetup
    {
        protected PipelineSetupContext Context;

        protected IJobService JobService => Context.JobService;

        public IStage Current { get; }

        internal PipelineSetup(IStage stage, PipelineSetupContext context)
        {
            Current = stage;
            Context = context;
        }
    }

    internal partial class PipelineSetup<TInput, TOutput> : PipelineSetup<TInput>, IPipelineSetup<TInput, TOutput>//, IStageSetupOut<TOutput>, IPipelineSetup<TInput, TOutput>
    {
        public new IStageOut<TOutput> Current => (IStageOut<TOutput>)base.Current;

        internal PipelineSetup(IStageOut<TOutput> stage, PipelineSetupContext context)
            : base(stage, context)
        { }

        #region Generic

        #region BulkStages

        public IPipelineSetup<TInput, TNextOutput> BulkStage<TBulkJob, TNextOutput>()
            where TBulkJob : BulkJob<TOutput, TNextOutput>
            => CreateNextBulkStage<TNextOutput>(JobService.GetJobInstance<TBulkJob>());

        public IPipelineSetup<TInput, TOutput> BulkStage<TBulkJob>()
            where TBulkJob : BulkJob<TOutput, TOutput>
            => CreateNextBulkStage<TOutput>(JobService.GetJobInstance<TBulkJob>());

        #endregion

        #region Stages

        public IPipelineSetup<TInput, TOutput> Stage<TJob>()
            where TJob : Job<TOutput, TOutput>
            => CreateNextStage<TOutput>(JobService.GetJobInstance<TJob>());

        public IPipelineSetup<TInput, TNextOutput> Stage<TJob, TNextOutput>()
            where TJob : Job<TOutput, TNextOutput>
            => CreateNextStage<TNextOutput>(JobService.GetJobInstance<TJob>());

        #endregion

        #endregion

        #region Nongeneric

        #region BulkStages

        public IPipelineSetup<TInput, TNextOutput> BulkStage<TNextOutput>(BulkJob<TOutput, TNextOutput> bulkJob)
            => CreateNextBulkStage(bulkJob);

        public IPipelineSetup<TInput, TNextOutput> BulkStage<TNextOutput>(Func<IEnumerable<TOutput>, IEnumerable<TNextOutput>> bulkFunc, BulkJobConfiguration bulkJobConfiguration)
            => BulkStage(new LambdaBulkJob<TOutput, TNextOutput>(bulkFunc, bulkJobConfiguration));

        public IPipelineSetup<TInput, TNextOutput> BulkStage<TNextOutput>(Func<IEnumerable<TOutput>, Task<IEnumerable<TNextOutput>>> bulkFunc, BulkJobConfiguration bulkJobConfiguration)
            => BulkStage(new LambdaBulkJob<TOutput, TNextOutput>(bulkFunc, bulkJobConfiguration));

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
            BroadcastBlock<PipelineItem<TOutput>> MakeNextBlock(StageCreationOptions options)
            {
                var broadcastBlock = new BroadcastBlock<PipelineItem<TOutput>>(x => x);

                var currentBlock = Current.RetrieveExecutionBlock(options);

                currentBlock.LinkTo(broadcastBlock);
                currentBlock.Completion.ContinueWith(x =>
                {
                    broadcastBlock.Complete();
                }, Context.CancellationToken);


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
            IPropagatorBlock<PipelineItem<TNextOutput>, PipelineItem<TNextOutput>> MakeNextBlock(StageCreationOptions options)
            {
                var mergeBlock = new TransformBlock<PipelineItem<TNextOutput>, PipelineItem<TNextOutput>>(x => x);

                IDataflowBlock[] headBranches = new IDataflowBlock[branches.Length];
                IDataflowBlock[] tailBranches = new IDataflowBlock[branches.Length];

                var branchId = 0;

                var currentBlock = Current.RetrieveExecutionBlock(options);
                foreach (var branch in branches)
                {
                    var newBranchHead = new TransformBlock<PipelineItem<TOutput>, PipelineItem<TOutput>>(x => x);

                    headBranches[branchId] = newBranchHead;

                    var newtBranchStageHead = new StageOut<TOutput>((d) => newBranchHead)
                    {
                        Previous = Current
                    };

                    Current.Next.Add(newtBranchStageHead);

                    var nextBlockAfterCurrent = new PipelineSetup<TInput, TOutput>(newtBranchStageHead, Context);

                    var newBranch = branch.branch(nextBlockAfterCurrent);

                    

                    currentBlock.LinkTo(newBranchHead,
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

                    var newBranchBlock = newBranch.Current.RetrieveExecutionBlock(options);

                    tailBranches[branchId] = newBranchBlock;

                    newBranchBlock.LinkTo(mergeBlock); //TODO broadcast TEST 

                    newBranchBlock.Completion.ContinueWith(x =>
                    {
                        if (tailBranches.All(tail => tail.Completion.IsCompleted))
                        {
                            mergeBlock.Complete();
                        }
                    }, Context.CancellationToken);

                    branchId++;
                }

                currentBlock.Completion.ContinueWith(x =>
                {
                    foreach (var headBranch in headBranches)
                    {
                        headBranch.Complete();
                    }
                }, Context.CancellationToken);

                return mergeBlock;
            };

            var nextStage = new StageOut<TNextOutput>(MakeNextBlock)//TODO
            {
                Previous = Current
            };

            Current.Next.Add(nextStage); // Hack with cross linking to destroy
            return new PipelineSetup<TInput, TNextOutput>(nextStage, Context);
        }

        #endregion

        public IPipelineSetup<TInput, TNextOutput> MergeWith<TNextOutput>(IPipelineSetup<TOutput, TNextOutput> pipelineSetup)
        {
            var nextBlock = pipelineSetup.GetFirstStage<TOutput>();

            ISourceBlock<PipelineItem<TNextOutput>> MakeNextBlock(StageCreationOptions options)
            {
                Current.Next.Add(nextBlock);
                nextBlock.Previous = Current;
                var currentBlock = Current.RetrieveExecutionBlock(options);

                currentBlock.LinkTo(nextBlock.RetrieveExecutionBlock(options), new DataflowLinkOptions() { PropagateCompletion = true });
                currentBlock.Completion.ContinueWith(x =>
                {
                    nextBlock.RetrieveExecutionBlock(options).Complete();
                }, Context.CancellationToken);

                currentBlock.Completion.ContinueWith(x =>
                {
                    nextBlock.RetrieveExecutionBlock(options).Complete();
                }, Context.CancellationToken);

                return pipelineSetup.Current.RetrieveExecutionBlock(options);
            };

            var nextStage = new StageOut<TNextOutput>(MakeNextBlock)
            {
                Previous = Current
            };

            Current.Next.Add(nextStage); // Hack with cross linking to destroy

            return new PipelineSetup<TInput, TNextOutput>(nextStage, Context);
        }

        public static PipelineSetup<TInput, TOutput> operator +(PipelineSetup<TInput, TOutput> pipelineSetup, PipelineSetup<TOutput, TOutput> pipelineSetup2)
        {
            var y =  pipelineSetup.MergeWith(pipelineSetup2);

            return (PipelineSetup<TInput, TOutput>) y;
        }

        public IAwaitablePipelineRunner<TInput, TOutput> CreateAwaitable(AwaitablePipelineConfig pipelineConfig = null)
        {
            IStageIn<TInput> firstStage = this.GetFirstStage<TInput>();
            return new AwaitablePipelineRunner<TInput, TOutput>(firstStage.RetrieveExecutionBlock, Current.RetrieveExecutionBlock, Context.CancellationToken, () => firstStage.DestroyStageBlocks(), pipelineConfig);
        }

        public IPipelineRunner<TInput, TOutput> Create()
        {
            IStageIn<TInput> firstStage = this.GetFirstStage<TInput>();
            return new PipelineRunner<TInput, TOutput>(firstStage.RetrieveExecutionBlock, Current.RetrieveExecutionBlock, Context.CancellationToken);
        }

        private PipelineSetup<TInput, TNextOutput> CreateNextStage<TNextOutput>(IPipelineJob<TOutput, TNextOutput> job)
        {
            IPropagatorBlock<PipelineItem<TOutput>, PipelineItem<TNextOutput>> MakeNextBlock(StageCreationOptions options)
            {

                TransformBlock<PipelineItem<TOutput>, PipelineItem<TNextOutput>> rePostBlock = null;

                void RePostMessage(PipelineItem<TOutput> message)
                {
                    rePostBlock?.Post(message);
                }

                var nextBlock = new TransformBlock<PipelineItem<TOutput>, PipelineItem<TNextOutput>>(
                    async x => await job.InternalExecute(x, Context.GetPipelineJobContext(() => RePostMessage(x))),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = job.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = job.Configuration.MaxMessagesPerTask,
                        CancellationToken = Context.CancellationToken
                    });

                rePostBlock = nextBlock;
                var currentBlock = Current.RetrieveExecutionBlock(options);

                currentBlock.LinkTo(nextBlock, new DataflowLinkOptions() { PropagateCompletion = true });
                currentBlock.Completion.ContinueWith(x =>
                {
                    //DiagnosticAction?.Invoke(new DiagnosticItem)
                    nextBlock.Complete();
                }, Context.CancellationToken);

                return nextBlock;
            }

            return CreateNextBlock(MakeNextBlock, job.Configuration);
        }

        private PipelineSetup<TInput, TNextOutput> CreateNextBulkStage<TNextOutput>(IPipelineBulkJob<TOutput, TNextOutput> job)
        {
            IPropagatorBlock<PipelineItem<TOutput>, PipelineItem<TNextOutput>> MakeNextBlock(StageCreationOptions options)
            {
                IPropagatorBlock<PipelineItem<TOutput>, PipelineItem<TOutput>[]> buffer;
                if (options.UseTimeOuts)
                {
                    buffer = new BatchBlockEx<PipelineItem<TOutput>>(job.Configuration.BatchItemsCount, job.Configuration.BatchItemsTimeOut); //TODO
                }
                else
                {
                    buffer = new BatchBlock<PipelineItem<TOutput>>(job.Configuration.BatchItemsCount); //TODO
                }

                TransformManyBlock<IEnumerable<PipelineItem<TOutput>>, PipelineItem<TNextOutput>> rePostBlock = null;

                void RePostMessages(IEnumerable<PipelineItem<TOutput>> messages)
                {
                    rePostBlock?.Post(messages);
                }

                var nextBlock = new TransformManyBlock<IEnumerable<PipelineItem<TOutput>>, PipelineItem<TNextOutput>>(
                    async x => await job.InternalExecute(x, Context.GetPipelineJobContext(() => RePostMessages(x))),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = job.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = job.Configuration.MaxMessagesPerTask,
                        CancellationToken = Context.CancellationToken
                    });

                buffer.LinkTo(nextBlock, new DataflowLinkOptions() { PropagateCompletion = true });
                rePostBlock = nextBlock;

                buffer.Completion.ContinueWith(x =>
                {
                    nextBlock.Complete();
                }, Context.CancellationToken);

                var next = DataflowBlock.Encapsulate(buffer, nextBlock);
                var currentBlock = Current.RetrieveExecutionBlock(options);

                currentBlock.LinkTo(next, new DataflowLinkOptions() { PropagateCompletion = true });

                currentBlock.Completion.ContinueWith(x =>
                {
                    next.Complete();
                }, Context.CancellationToken);

                return next;
            }

            return CreateNextBlock(MakeNextBlock, job.Configuration);
        }

        private PipelineSetup<TInput, TNextOutput> CreateNextBlock<TNextOutput>(Func<StageCreationOptions, IPropagatorBlock<PipelineItem<TOutput>, PipelineItem<TNextOutput>>> nextBlock, PipelineBaseConfiguration pipelineBaseConfiguration)
        {
            var nextStage = new StageOut<TNextOutput>(nextBlock)
            {
                Previous = Current,
                PipelineBaseConfiguration = pipelineBaseConfiguration
            };

            Current.Next.Add(nextStage);

            return new PipelineSetup<TInput, TNextOutput>(nextStage, Context);
        }
    }
}