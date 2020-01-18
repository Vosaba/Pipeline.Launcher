﻿using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Extensions;
using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Abstractions.PipelineRunner;
using PipelineLauncher.Abstractions.PipelineRunner.Configurations;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using PipelineLauncher.PipelineRunner;
using PipelineLauncher.PipelineStage;
using PipelineLauncher.Stages;
using PipelineLauncher.StageSetup;
using System.Collections.Concurrent;

namespace PipelineLauncher.PipelineSetup
{
    internal abstract class PipelineSetup<TFirstInput> : IPipelineSetup
    {
        protected PipelineSetupContext Context;

        protected IStageService StageService => Context.StageService;

        public IStageSetup Current { get; }

        internal PipelineSetup(IStageSetup stageSetup, PipelineSetupContext context)
        {
            Current = stageSetup;
            Context = context;
        }
    }

    internal partial class PipelineSetup<TInput, TOutput> : PipelineSetup<TInput>, IPipelineSetup<TInput, TOutput>
    {
        public new IStageSetupOut<TOutput> Current => (IStageSetupOut<TOutput>)base.Current;

        internal PipelineSetup(IStageSetupOut<TOutput> stageSetup, PipelineSetupContext context)
            : base(stageSetup, context)
        { }

        #region Generic

        #region BulkStages

        public IPipelineSetup<TInput, TNextOutput> BulkStage<TBulkStage, TNextOutput>()
            where TBulkStage : BulkStage<TOutput, TNextOutput>
            => CreateNextBulkStage<TNextOutput>(StageService.GetStageInstance<TBulkStage>());

        public IPipelineSetup<TInput, TOutput> BulkStage<TBulkStage>()
            where TBulkStage : BulkStage<TOutput, TOutput>
            => CreateNextBulkStage<TOutput>(StageService.GetStageInstance<TBulkStage>());

        #endregion

        #region Stages

        public IPipelineSetup<TInput, TOutput> Stage<TStage>()
            where TStage : Stages.Stage<TOutput, TOutput>
            => CreateNextStage<TOutput>(StageService.GetStageInstance<TStage>());

        public IPipelineSetup<TInput, TNextOutput> Stage<TStage, TNextOutput>()
            where TStage : Stages.Stage<TOutput, TNextOutput>
            => CreateNextStage<TNextOutput>(StageService.GetStageInstance<TStage>());

        #endregion

        #endregion

        #region Nongeneric

        #region BulkStages

        public IPipelineSetup<TInput, TNextOutput> BulkStage<TNextOutput>(BulkStage<TOutput, TNextOutput> bulkStage)
            => CreateNextBulkStage(bulkStage);

        public IPipelineSetup<TInput, TNextOutput> BulkStage<TNextOutput>(Func<IEnumerable<TOutput>, IEnumerable<TNextOutput>> bulkFunc, BulkStageConfiguration bulkStageConfiguration)
            => BulkStage(new LambdaBulkStage<TOutput, TNextOutput>(bulkFunc, bulkStageConfiguration));

        public IPipelineSetup<TInput, TNextOutput> BulkStage<TNextOutput>(Func<IEnumerable<TOutput>, Task<IEnumerable<TNextOutput>>> bulkFunc, BulkStageConfiguration bulkStageConfiguration)
            => BulkStage(new LambdaBulkStage<TOutput, TNextOutput>(bulkFunc, bulkStageConfiguration));

        #endregion

        #region Stages

        public IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Stages.Stage<TOutput, TNextOutput> stage)
            => CreateNextStage(stage);

        public IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Func<TOutput, TNextOutput> func)
            => Stage(new LambdaStage<TOutput, TNextOutput>(func));

        public IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Func<TOutput, StageOption<TOutput, TNextOutput>, TNextOutput> funcWithOption)
            => Stage(new LambdaStage<TOutput, TNextOutput>(funcWithOption));

        public IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Func<TOutput, Task<TNextOutput>> func)
            => Stage(new LambdaStage<TOutput, TNextOutput>(func));

        public IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Func<TOutput, StageOption<TOutput, TNextOutput>, Task<TNextOutput>> funcWithOption)
            => Stage(new LambdaStage<TOutput, TNextOutput>(funcWithOption));

        #endregion

        #endregion

        #region Nongeneric Branch

        public IPipelineSetup<TInput, TNextOutput> Broadcast<TNextOutput>(params (Predicate<TOutput> predicate,
            Func<IPipelineSetup<TInput, TOutput>, IPipelineSetup<TInput, TNextOutput>> branch)[] branches)
        {
            BroadcastBlock<PipelineStageItem<TOutput>> MakeNextBlock(StageCreationOptions options)
            {
                var broadcastBlock = new BroadcastBlock<PipelineStageItem<TOutput>>(x => x);

                var currentBlock = Current.RetrieveExecutionBlock(options);

                currentBlock.LinkTo(broadcastBlock, new DataflowLinkOptions { PropagateCompletion = false} );
                currentBlock.Completion.ContinueWith(x =>
                {
                    broadcastBlock.Complete();
                });//, Context.CancellationToken);


                return broadcastBlock;

            }

            var newCurrent = CreateNextBlock(MakeNextBlock, Current.PipelineBaseConfiguration);

            //return PipelineSetupExtensions.RemoveDuplicates(newCurrent.Branch(branches));
            return newCurrent.Branch(branches).RemoveDuplicatesPermanent();
        }

        public IPipelineSetup<TInput, TNextOutput> Branch<TNextOutput>((Predicate<TOutput> predicate,
            Func<IPipelineSetup<TInput, TOutput>, IPipelineSetup<TInput, TNextOutput>> branch)[] branches)
        {
            return Branch(ConditionExceptionScenario.GoToNextCondition, branches);
        }

        public IPipelineSetup<TInput, TNextOutput> Branch<TNextOutput>(ConditionExceptionScenario conditionExceptionScenario, (Predicate<TOutput> predicate, Func<IPipelineSetup<TInput, TOutput>, IPipelineSetup<TInput, TNextOutput>> branch)[] branches)
        {
            IPropagatorBlock<PipelineStageItem<TNextOutput>, PipelineStageItem<TNextOutput>> MakeNextBlock(StageCreationOptions options)
            {
                var mergeBlock = new TransformBlock<PipelineStageItem<TNextOutput>, PipelineStageItem<TNextOutput>>(x => x);

                IDataflowBlock[] headBranches = new IDataflowBlock[branches.Length];
                IDataflowBlock[] tailBranches = new IDataflowBlock[branches.Length];

                var branchId = 0;

                var currentBlock = Current.RetrieveExecutionBlock(options);
                foreach (var branch in branches)
                {
                    var newBranchHead = new TransformBlock<PipelineStageItem<TOutput>, PipelineStageItem<TOutput>>(x => x);

                    headBranches[branchId] = newBranchHead;

                    var newtBranchStageHead = new StageSetupOut<TOutput>((d) => newBranchHead)
                    {
                        Previous = Current
                    };

                    Current.Next.Add(newtBranchStageHead);

                    var nextBlockAfterCurrent = new PipelineSetup<TInput, TOutput>(newtBranchStageHead, Context);

                    var newBranch = branch.branch(nextBlockAfterCurrent);



                    currentBlock.LinkTo(newBranchHead, new DataflowLinkOptions { PropagateCompletion = false },
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
                                        mergeBlock.Post(new ExceptionStageItem<TNextOutput>(ex, null, branch.predicate.GetType(), x.Item));
                                        return false;
                                    case ConditionExceptionScenario.BreakPipelineExecution:
                                    default:
                                        throw;
                                }
                            }
                        }); ;

                    var newBranchBlock = newBranch.Current.RetrieveExecutionBlock(options);

                    tailBranches[branchId] = newBranchBlock;

                    newBranchBlock.LinkTo(mergeBlock, new DataflowLinkOptions { PropagateCompletion = false } ); //TODO broadcast TEST 

                    newBranchBlock.Completion.ContinueWith(x =>
                    {
                        if (tailBranches.All(tail => tail.Completion.IsCompleted))
                        {
                            mergeBlock.Complete();
                        }
                    });//, Context.CancellationToken);

                    branchId++;
                }

                currentBlock.Completion.ContinueWith(x =>
                {
                    foreach (var headBranch in headBranches)
                    {
                        headBranch.Complete();
                    }
                });//, Context.CancellationToken);

                return mergeBlock;
            };

            var nextStage = new StageSetupOut<TNextOutput>(MakeNextBlock)//TODO
            {
                Previous = Current
            };

            Current.Next.Add(nextStage); // Hack with cross linking to destroy
            return new PipelineSetup<TInput, TNextOutput>(nextStage, Context);
        }

        #endregion

        public IPipelineSetup<TInput, TOutput> RemoveDuplicates()
        {
            TransformBlock<PipelineStageItem<TOutput>, PipelineStageItem<TOutput>> MakeNextBlock(StageCreationOptions options)
            {
                var mergeBlock = new TransformBlock<PipelineStageItem<TOutput>, PipelineStageItem<TOutput>>(x =>
                {
                    return x;
                });

                var currentBlock = Current.RetrieveExecutionBlock(options);

                var processedHash = new ConcurrentDictionary<int, byte>();

                currentBlock.LinkTo(mergeBlock);

                currentBlock.Completion.ContinueWith(x => mergeBlock.Complete());//, Context.CancellationToken);

                return mergeBlock;
            }

            return CreateNextBlock(MakeNextBlock, Current.PipelineBaseConfiguration);
        }

        public IPipelineSetup<TInput, TNextOutput> MergeWith<TNextOutput>(IPipelineSetup<TOutput, TNextOutput> pipelineSetup)
        {
            var nextBlock = pipelineSetup.GetFirstStage<TOutput>();

            ISourceBlock<PipelineStageItem<TNextOutput>> MakeNextBlock(StageCreationOptions options)
            {
               
                Current.Next.Add(nextBlock);
                nextBlock.Previous = Current;
                var currentBlock = Current.RetrieveExecutionBlock(options);

                currentBlock.LinkTo(nextBlock.RetrieveExecutionBlock(options), new DataflowLinkOptions() { PropagateCompletion = false });
                currentBlock.Completion.ContinueWith(x =>
                {
                    nextBlock.RetrieveExecutionBlock(options).Complete();
                });//, Context.CancellationToken);

                currentBlock.Completion.ContinueWith(x =>
                {
                    nextBlock.RetrieveExecutionBlock(options).Complete();
                });//, Context.CancellationToken);

                return pipelineSetup.Current.RetrieveExecutionBlock(options);
            };

            var nextStage = new StageSetupOut<TNextOutput>(MakeNextBlock)
            {
                Previous = Current
            };

            Current.Next.Add(nextStage); // Hack with cross linking to destroy

            return new PipelineSetup<TInput, TNextOutput>(nextStage, Context);
        }

        //public static PipelineSetup<TInput, TOutput> operator +(PipelineSetup<TInput, TOutput> pipelineSetup, PipelineSetup<TOutput, TOutput> pipelineSetup2)
        //{
        //    var y =  pipelineSetup.MergeWith(pipelineSetup2);

        //    return (PipelineSetup<TInput, TOutput>) y;
        //}

        public IAwaitablePipelineRunner<TInput, TOutput> CreateAwaitable(AwaitablePipelineConfig pipelineConfig = null)
        {
            IStageSetupIn<TInput> firstStageSetup = this.GetFirstStage<TInput>();
            return new AwaitablePipelineRunner<TInput, TOutput>(firstStageSetup.RetrieveExecutionBlock, Current.RetrieveExecutionBlock, Context, () => firstStageSetup.DestroyStageBlocks(), pipelineConfig);
        }

        public IPipelineRunner<TInput, TOutput> Create(PipelineConfig pipelineConfig = null)
        {
            IStageSetupIn<TInput> firstStageSetup = this.GetFirstStage<TInput>();
            return new PipelineRunner<TInput, TOutput>(firstStageSetup.RetrieveExecutionBlock, Current.RetrieveExecutionBlock, Context, pipelineConfig);
        }

        private PipelineSetup<TInput, TNextOutput> CreateNextStage<TNextOutput>(IPipelineStage<TOutput, TNextOutput> stage)
        {
            IPropagatorBlock<PipelineStageItem<TOutput>, PipelineStageItem<TNextOutput>> MakeNextBlock(StageCreationOptions options)
            {

                TransformBlock<PipelineStageItem<TOutput>, PipelineStageItem<TNextOutput>> rePostBlock = null;

                void RePostMessage(PipelineStageItem<TOutput> message)
                {
                    rePostBlock?.Post(message);
                }

                var nextBlock = new TransformBlock<PipelineStageItem<TOutput>, PipelineStageItem<TNextOutput>>(
                    async x => await stage.InternalExecute(x, Context.GetPipelineStageContext(() => RePostMessage(x))),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = stage.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = stage.Configuration.MaxMessagesPerTask,
                        CancellationToken = Context.CancellationToken,
                        SingleProducerConstrained = stage.Configuration.SingleProducerConstrained
                    });

                rePostBlock = nextBlock;
                var currentBlock = Current.RetrieveExecutionBlock(options);

                currentBlock.LinkTo(nextBlock, new DataflowLinkOptions() { PropagateCompletion = false });
                currentBlock.Completion.ContinueWith(x =>
                {
                    //DiagnosticAction?.Invoke(new DiagnosticItem)
                    nextBlock.Complete();
                });//, Context.CancellationToken);

                return nextBlock;
            }

            return CreateNextBlock(MakeNextBlock, stage.Configuration);
        }

        private PipelineSetup<TInput, TNextOutput> CreateNextBulkStage<TNextOutput>(IPipelineBulkStage<TOutput, TNextOutput> stage)
        {
            IPropagatorBlock<PipelineStageItem<TOutput>, PipelineStageItem<TNextOutput>> MakeNextBlock(StageCreationOptions options)
            {
                IPropagatorBlock<PipelineStageItem<TOutput>, PipelineStageItem<TOutput>[]> buffer;
                if (options.UseTimeOuts)
                {
                    buffer = new BatchBlockEx<PipelineStageItem<TOutput>>(stage.Configuration.BatchItemsCount, stage.Configuration.BatchItemsTimeOut); //TODO
                }
                else
                {
                    buffer = new BatchBlock<PipelineStageItem<TOutput>>(stage.Configuration.BatchItemsCount); //TODO
                }

                TransformManyBlock<IEnumerable<PipelineStageItem<TOutput>>, PipelineStageItem<TNextOutput>> rePostBlock = null;

                void RePostMessages(IEnumerable<PipelineStageItem<TOutput>> messages)
                {
                    rePostBlock?.Post(messages);
                }

                var nextBlock = new TransformManyBlock<IEnumerable<PipelineStageItem<TOutput>>, PipelineStageItem<TNextOutput>>(
                    async x => await stage.InternalExecute(x, Context.GetPipelineStageContext(() => RePostMessages(x))),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = stage.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = stage.Configuration.MaxMessagesPerTask,
                        CancellationToken = Context.CancellationToken,
                        SingleProducerConstrained = stage.Configuration.SingleProducerConstrained
                    });

                buffer.LinkTo(nextBlock, new DataflowLinkOptions() { PropagateCompletion = false });
                rePostBlock = nextBlock;

                buffer.Completion.ContinueWith(x =>
                {
                    nextBlock.Complete();
                });//, Context.CancellationToken);

                var next = DataflowBlock.Encapsulate(buffer, nextBlock);
                var currentBlock = Current.RetrieveExecutionBlock(options);

                currentBlock.LinkTo(next, new DataflowLinkOptions() { PropagateCompletion = false });

                currentBlock.Completion.ContinueWith(x =>
                {
                    next.Complete();
                });//, Context.CancellationToken);

                return next;
            }

            return CreateNextBlock(MakeNextBlock, stage.Configuration);
        }

        private PipelineSetup<TInput, TNextOutput> CreateNextBlock<TNextOutput>(Func<StageCreationOptions, IPropagatorBlock<PipelineStageItem<TOutput>, PipelineStageItem<TNextOutput>>> nextBlock, PipelineBaseConfiguration pipelineBaseConfiguration)
        {
            var nextStage = new StageSetupOut<TNextOutput>(nextBlock)
            {
                Previous = Current,
                PipelineBaseConfiguration = pipelineBaseConfiguration
            };

            Current.Next.Add(nextStage);

            return new PipelineSetup<TInput, TNextOutput>(nextStage, Context);
        }
    }
}