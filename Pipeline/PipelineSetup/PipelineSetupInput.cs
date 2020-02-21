using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineRunner;
using PipelineLauncher.Abstractions.PipelineRunner.Configurations;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.Abstractions.Stages;
using PipelineLauncher.Blocks;
using PipelineLauncher.Exceptions;
using PipelineLauncher.PipelineRunner;
using PipelineLauncher.PipelineStage;
using PipelineLauncher.Stages;
using PipelineLauncher.StageSetup;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace PipelineLauncher.PipelineSetup
{
    internal abstract class PipelineSetup<TPipelineInput> : IPipelineSetup
    {
        public IStageSetup StageSetup { get; }

        protected PipelineCreationContext PipelineCreationContext;
        protected IStageService StageService => PipelineCreationContext.StageService;


        internal PipelineSetup(IStageSetup stageSetup, PipelineCreationContext pipelineCreationContext)
        {
            StageSetup = stageSetup;
            PipelineCreationContext = pipelineCreationContext;
        }
    }


    internal partial class PipelineSetup<TPipelineInput, TStageOutput> : PipelineSetup<TPipelineInput>, IPipelineSetup<TPipelineInput, TStageOutput>
    {
        public IStageSetupOut<TStageOutput> StageSetupOut => (IStageSetupOut<TStageOutput>)StageSetup;

        internal PipelineSetup(IStageSetupOut<TStageOutput> stageSetupOut, PipelineCreationContext pipelineCreationContext)
            : base(stageSetupOut, pipelineCreationContext)
        { }

        #region Generic

        #region BulkStages

        public IPipelineSetup<TPipelineInput, TNextStageOutput> BulkStage<TBulkStage, TNextStageOutput>(PipelinePredicate<TStageOutput> predicate = null)
            where TBulkStage : class, IBulkStage<TStageOutput, TNextStageOutput>
            => CreateBulkStage(StageService.GetStageInstance<TBulkStage>(), predicate);

        public IPipelineSetup<TPipelineInput, TStageOutput> BulkStage<TBulkStage>(PipelinePredicate<TStageOutput> predicate = null)
            where TBulkStage : class, IBulkStage<TStageOutput, TStageOutput>
            => CreateBulkStage(StageService.GetStageInstance<TBulkStage>(), predicate);

        #endregion

        #region Stages

        public IPipelineSetup<TPipelineInput, TStageOutput> Stage<TStage>(PipelinePredicate<TStageOutput> predicate = null)
            where TStage : class, IStage<TStageOutput, TStageOutput>
            => CreateStage(StageService.GetStageInstance<TStage>(), predicate);

        public IPipelineSetup<TPipelineInput, TNextStageOutput> Stage<TStage, TNextStageOutput>(PipelinePredicate<TStageOutput> predicate = null)
            where TStage : class, IStage<TStageOutput, TNextStageOutput>
            => CreateStage(StageService.GetStageInstance<TStage>(), predicate);

        #endregion

        #endregion

        #region Nongeneric

        #region BulkStages

        public IPipelineSetup<TPipelineInput, TNextStageOutput> BulkStage<TNextStageOutput>(IBulkStage<TStageOutput, TNextStageOutput> bulkStage, PipelinePredicate<TStageOutput> predicate = null)
            => CreateBulkStage(bulkStage, predicate);

        public IPipelineSetup<TPipelineInput, TNextStageOutput> BulkStage<TNextStageOutput>(Func<TStageOutput[], IEnumerable<TNextStageOutput>> func, BulkStageConfiguration bulkStageConfiguration)
            => BulkStage(new LambdaBulkStage<TStageOutput, TNextStageOutput>(func, bulkStageConfiguration));

        public IPipelineSetup<TPipelineInput, TNextStageOutput> BulkStage<TNextStageOutput>(Func<TStageOutput[], Task<IEnumerable<TNextStageOutput>>> func, BulkStageConfiguration bulkStageConfiguration)
            => BulkStage(new LambdaBulkStage<TStageOutput, TNextStageOutput>(func, bulkStageConfiguration));

        #endregion

        #region Stages

        public IPipelineSetup<TPipelineInput, TNextStageOutput> Stage<TNextStageOutput>(IStage<TStageOutput, TNextStageOutput> stage, PipelinePredicate<TStageOutput> predicate = null)
            => CreateStage(stage, predicate);

        public IPipelineSetup<TPipelineInput, TNextStageOutput> Stage<TNextStageOutput>(Func<TStageOutput, TNextStageOutput> func)
            => Stage(new LambdaStage<TStageOutput, TNextStageOutput>(func));

        public IPipelineSetup<TPipelineInput, TNextStageOutput> Stage<TNextStageOutput>(Func<TStageOutput, StageOption<TStageOutput, TNextStageOutput>, TNextStageOutput> func)
            => Stage(new LambdaStage<TStageOutput, TNextStageOutput>(func));

        public IPipelineSetup<TPipelineInput, TNextStageOutput> Stage<TNextStageOutput>(Func<TStageOutput, Task<TNextStageOutput>> func)
            => Stage(new LambdaStage<TStageOutput, TNextStageOutput>(func));

        public IPipelineSetup<TPipelineInput, TNextStageOutput> Stage<TNextStageOutput>(Func<TStageOutput, StageOption<TStageOutput, TNextStageOutput>, Task<TNextStageOutput>> func)
            => Stage(new LambdaStage<TStageOutput, TNextStageOutput>(func));

        #endregion

        #endregion

        #region Nongeneric Branch

        public IPipelineSetup<TPipelineInput, TNextStageOutput> Broadcast<TNextStageOutput>(
            params (Predicate<TStageOutput> predicate, 
                Func<IPipelineSetup<TPipelineInput, TStageOutput>, IPipelineSetup<TPipelineInput, TNextStageOutput>> branch)[] branches)
        {
            BroadcastBlock<PipelineStageItem<TStageOutput>> MakeNextBlock(StageCreationContext stageCreationContext)
            {
                var broadcastBlock = new BroadcastBlock<PipelineStageItem<TStageOutput>>(x => x);
                var sourceBlock = StageSetupOut.RetrieveExecutionBlock(stageCreationContext);

                sourceBlock.LinkTo(broadcastBlock, new DataflowLinkOptions { PropagateCompletion = false} );
                sourceBlock.Completion.ContinueWith(x => broadcastBlock.Complete());


                return broadcastBlock;
            }

            var newCurrent = CreatePipelineSetup(MakeNextBlock);

            return ((PipelineSetup<TPipelineInput, TNextStageOutput>)newCurrent.Branch(branches)).RemoveDuplicates(branches.Length);
        }

        public IPipelineSetup<TPipelineInput, TNextStageOutput> Branch<TNextStageOutput>(
            (Predicate<TStageOutput> predicate, 
                Func<IPipelineSetup<TPipelineInput, TStageOutput>, IPipelineSetup<TPipelineInput, TNextStageOutput>> branch)[] branches) 
            => Branch(ConditionExceptionScenario.GoToNextCondition, branches);

        public IPipelineSetup<TPipelineInput, TNextStageOutput> Branch<TNextStageOutput>(
            ConditionExceptionScenario conditionExceptionScenario,
            (Predicate<TStageOutput> predicate, Func<IPipelineSetup<TPipelineInput, TStageOutput>, IPipelineSetup<TPipelineInput, TNextStageOutput>> branch)[] branches)
        {
            IPropagatorBlock<PipelineStageItem<TNextStageOutput>, PipelineStageItem<TNextStageOutput>> CreateExecutionBlock(StageCreationContext options)
            {
                var sourceBlock = StageSetupOut.RetrieveExecutionBlock(options);
                var targetBlock = new TransformBlock<PipelineStageItem<TNextStageOutput>, PipelineStageItem<TNextStageOutput>>(x => x);

                var branchId = 0;
                IDataflowBlock[] headBranches = new IDataflowBlock[branches.Length];
                IDataflowBlock[] tailBranches = new IDataflowBlock[branches.Length];

                foreach (var (predicate, branch) in branches)
                {
                    var headBranchBlock = new TransformBlock<PipelineStageItem<TStageOutput>, PipelineStageItem<TStageOutput>>(x => x);
                    var branchHeadPipelineSetup = CreatePipelineSetup(CreateStageSetupOut(x => headBranchBlock));

                    var newBranchPipelineSetup = branch(branchHeadPipelineSetup);

                    sourceBlock.LinkTo(headBranchBlock, new DataflowLinkOptions { PropagateCompletion = false },
                        x =>
                        {
                            if (x == null)
                            {
                                return false;
                            }

                            if (x.Item == null)
                            {
                                return true;
                            }

                            try
                            {
                                return predicate(x.Item);
                            }
                            catch (Exception ex)
                            {
                                switch (conditionExceptionScenario)
                                {
                                    case ConditionExceptionScenario.GoToNextCondition:
                                        return false;
                                    case ConditionExceptionScenario.AddExceptionAndGoToNextCondition:
                                        targetBlock.Post(new ExceptionStageItem<TNextStageOutput>(ex, null, predicate.GetType(), x.Item));
                                        return false;
                                    case ConditionExceptionScenario.StopPipelineExecution:
                                    default:
                                        throw;
                                }
                            }
                        });

                    var tailBranchBlock = newBranchPipelineSetup.StageSetupOut.RetrieveExecutionBlock(options);

                    headBranches[branchId] = headBranchBlock;
                    tailBranches[branchId] = tailBranchBlock;

                    tailBranchBlock.LinkTo(targetBlock, new DataflowLinkOptions { PropagateCompletion = false } ); 
                    tailBranchBlock.Completion.ContinueWith(x =>
                    {
                        if (tailBranches.All(tail => tail.Completion.IsCompleted))
                        {
                            targetBlock.Complete();
                        }
                    });

                    branchId++;
                }

                sourceBlock.LinkTo(DataflowBlock.NullTarget<PipelineStageItem<TStageOutput>>(), new DataflowLinkOptions() { PropagateCompletion = false });
                sourceBlock.Completion.ContinueWith(x =>
                {
                    foreach (var headBranch in headBranches)
                    {
                        headBranch.Complete();
                    }
                });

                return targetBlock;
            };

            return CreatePipelineSetup(CreateExecutionBlock);
        }

        #endregion

        public IPipelineSetup<TPipelineInput, TNextStageOutput> MergeWith<TNextStageOutput>(IPipelineSetup<TStageOutput, TNextStageOutput> pipelineSetup)
        {
            var nextBlock = pipelineSetup.GetFirstStage<TStageOutput>();

            ISourceBlock<PipelineStageItem<TNextStageOutput>> CreateExecutionBlock(StageCreationContext options)
            {
                StageSetupOut.NextStageSetup.Add(nextBlock);
                nextBlock.PreviousStageSetup = StageSetupOut;
                var currentBlock = StageSetupOut.RetrieveExecutionBlock(options);

                currentBlock.LinkTo(nextBlock.RetrieveExecutionBlock(options), new DataflowLinkOptions() { PropagateCompletion = false });
                currentBlock.Completion.ContinueWith(x =>
                {
                    nextBlock.RetrieveExecutionBlock(options).Complete();
                });

                return pipelineSetup.StageSetupOut.RetrieveExecutionBlock(options);
            };

            var nextStage = new StageSetupOut<TNextStageOutput>(CreateExecutionBlock)
            {
                PreviousStageSetup = StageSetupOut
            };

            StageSetupOut.NextStageSetup.Add(nextStage); // Hack with cross linking to destroy

            return new PipelineSetup<TPipelineInput, TNextStageOutput>(nextStage, PipelineCreationContext);
        }

        public IAwaitablePipelineRunner<TPipelineInput, TStageOutput> CreateAwaitable(AwaitablePipelineCreationConfig pipelineCreationConfig = null)
        {
            if (pipelineCreationConfig == null)
            {
                pipelineCreationConfig = new AwaitablePipelineCreationConfig();
            }

            var firstStage = this.GetFirstStage<TPipelineInput>();
            return new AwaitablePipelineRunner<TPipelineInput, TStageOutput>(firstStage.RetrieveExecutionBlock, StageSetupOut.RetrieveExecutionBlock, () => firstStage.DestroyStageBlocks(), pipelineCreationConfig);
        }

        public IPipelineRunner<TPipelineInput, TStageOutput> Create(PipelineCreationConfig pipelineCreationConfig = null)
        {
            if (pipelineCreationConfig == null)
            {
                pipelineCreationConfig = new PipelineCreationConfig();
            }

            var firstStage = this.GetFirstStage<TPipelineInput>();
            return new PipelineRunner<TPipelineInput, TStageOutput>(firstStage.RetrieveExecutionBlock, StageSetupOut.RetrieveExecutionBlock, pipelineCreationConfig);
        }

        public IPipelineSetup<TPipelineInput, TStageOutput> RemoveDuplicates(int totalOccurrences)
        {
            IPropagatorBlock<PipelineStageItem<TStageOutput>, PipelineStageItem<TStageOutput>> CreateExecutionBlock(StageCreationContext stageCreationContext)
            {
                var processedHash = new ConcurrentDictionary<object, int>();

                var buffer = new BatchBlock<PipelineStageItem<TStageOutput>>(1); //TODO
                //var pipelineCreationContext = PipelineCreationContext.GetPipelineStageContext(null);


                IEnumerable<PipelineStageItem<TStageOutput>> Filter(IEnumerable<PipelineStageItem<TStageOutput>> items)
                {
                    foreach (var item in items)
                    {
                        object originalItem;
                        switch (item)
                        {
                            case NonResultStageItem<TPipelineInput> noneResultItem:
                                originalItem = noneResultItem.OriginalItem;
                                break;
                            default:
                                originalItem = item.Item;
                                break;
                        }

                        if (processedHash.TryAdd(originalItem, 1))
                        {
                            yield return item;
                        }
                        else
                        {
                            processedHash[originalItem]++;
                        }

                        if (processedHash[originalItem] == totalOccurrences)
                        {
                            processedHash.Remove(originalItem, out _);
                        }
                    }
                }

                var mergeBlock = new TransformManyBlock<IEnumerable<PipelineStageItem<TStageOutput>>, PipelineStageItem<TStageOutput>>(Filter);

                buffer.LinkTo(mergeBlock);
                buffer.Completion.ContinueWith(x => mergeBlock.Complete());

                var next = DataflowBlock.Encapsulate(buffer, mergeBlock);

                var currentBlock = StageSetupOut.RetrieveExecutionBlock(stageCreationContext);

                currentBlock.LinkTo(next);

                currentBlock.Completion.ContinueWith(x => next.Complete());//, PipelineCreationContext.CancellationToken);


                return next;
            }

            return CreatePipelineSetup(CreateExecutionBlock);
        }

        private PipelineSetup<TPipelineInput, TNextStageOutput> CreateStage<TNextStageOutput>(IStage<TStageOutput, TNextStageOutput> stage, PipelinePredicate<TStageOutput> predicate)
        {
            IPropagatorBlock<PipelineStageItem<TStageOutput>, PipelineStageItem<TNextStageOutput>> CreateExecutionBlock(StageCreationContext stageCreationContext)
            {
                var pipelineStage = new PipelineStage<TStageOutput, TNextStageOutput>(stage);
                TransformBlock<PipelineStageItem<TStageOutput>, PipelineStageItem<TNextStageOutput>> nextBlock = null;

                Action<PipelineStageItem<TStageOutput>> RePostMessage;
                if (stageCreationContext.PipelineType == PipelineType.Normal)
                {
                    RePostMessage = message => nextBlock?.Post(message);
                }
                else
                {
                    RePostMessage = message => throw new PipelineUsageException(Helpers.Strings.RetryOnAwaitable);
                }

                nextBlock = new TransformBlock<PipelineStageItem<TStageOutput>, PipelineStageItem<TNextStageOutput>>(
                    async x => await pipelineStage.BaseExecute(x, stageCreationContext.GetPipelineStageContext(() => RePostMessage(x))),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = pipelineStage.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = pipelineStage.Configuration.MaxMessagesPerTask,
                        CancellationToken = stageCreationContext.CancellationToken,
                        SingleProducerConstrained = pipelineStage.Configuration.SingleProducerConstrained,
                        EnsureOrdered = pipelineStage.Configuration.EnsureOrdered
                    });

                var sourceBlock = StageSetupOut.RetrieveExecutionBlock(stageCreationContext);

                if (predicate != null || pipelineStage is IConditionalStage<TStageOutput>)
                {
                    if(pipelineStage is IConditionalStage<TStageOutput> stageCondition)
                    {
                        predicate = stageCondition.Predicate;
                    }

                    sourceBlock.LinkTo(nextBlock, new DataflowLinkOptions { PropagateCompletion = false }, predicate.GetPredicate(nextBlock));
                    sourceBlock.LinkTo(DataflowBlock.NullTarget<PipelineStageItem<TStageOutput>>(), new DataflowLinkOptions { PropagateCompletion = false });
                }
                else
                {
                    sourceBlock.LinkTo(nextBlock, new DataflowLinkOptions { PropagateCompletion = false });
                }

                sourceBlock.Completion.ContinueWith(x => nextBlock.Complete());

                return nextBlock;
            }

            return CreatePipelineSetup(CreateExecutionBlock);
        }

        private PipelineSetup<TPipelineInput, TNextStageOutput> CreateBulkStage<TNextStageOutput>(IBulkStage<TStageOutput, TNextStageOutput> bulkStage, PipelinePredicate<TStageOutput> predicate)
        {
            IPropagatorBlock<PipelineStageItem<TStageOutput>, PipelineStageItem<TNextStageOutput>> CreateExecutionBlock(StageCreationContext stageCreationContext)
            {
                var pipelineBulkStage = new PipelineBulkStage<TStageOutput, TNextStageOutput>(bulkStage);
                TransformManyBlock<IEnumerable<PipelineStageItem<TStageOutput>>, PipelineStageItem<TNextStageOutput>> nextBlock = null;

                IPropagatorBlock<PipelineStageItem<TStageOutput>, PipelineStageItem<TStageOutput>[]> batchPrepareBlock;
                if (stageCreationContext.UseTimeOut)
                {
                    batchPrepareBlock = new BatchBlockWithTimeOut<PipelineStageItem<TStageOutput>>(pipelineBulkStage.Configuration.BatchSize, pipelineBulkStage.Configuration.BatchTimeOut); //TODO
                }
                else
                {
                    batchPrepareBlock = new BatchBlock<PipelineStageItem<TStageOutput>>(pipelineBulkStage.Configuration.BatchSize); //TODO
                }

                Action<IEnumerable<PipelineStageItem<TStageOutput>>> RePostMessages;
                if (stageCreationContext.PipelineType == PipelineType.Normal)
                {
                    RePostMessages = messages => nextBlock?.Post(messages);
                }
                else
                {
                    RePostMessages = messages => throw new PipelineUsageException(Helpers.Strings.RetryOnAwaitable);
                }

                nextBlock = new TransformManyBlock<IEnumerable<PipelineStageItem<TStageOutput>>, PipelineStageItem<TNextStageOutput>>(
                    async x => await pipelineBulkStage.BaseExecute(x, stageCreationContext.GetPipelineStageContext(() => RePostMessages(x))),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = pipelineBulkStage.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = pipelineBulkStage.Configuration.MaxMessagesPerTask,
                        CancellationToken = stageCreationContext.CancellationToken,
                        SingleProducerConstrained = pipelineBulkStage.Configuration.SingleProducerConstrained,
                        EnsureOrdered = pipelineBulkStage.Configuration.EnsureOrdered
                    });

                batchPrepareBlock.LinkTo(nextBlock, new DataflowLinkOptions() { PropagateCompletion = false });

                batchPrepareBlock.Completion.ContinueWith(x =>
                {
                    nextBlock.Complete();
                });

                var currentBlock = StageSetupOut.RetrieveExecutionBlock(stageCreationContext);
                var targetBlock = DataflowBlock.Encapsulate(batchPrepareBlock, nextBlock);

                if (predicate != null || pipelineBulkStage is IConditionalStage<TStageOutput>)
                {
                    if (pipelineBulkStage is IConditionalStage<TStageOutput> stageCondition)
                    {
                        predicate = stageCondition.Predicate;
                    }

                    currentBlock.LinkTo(targetBlock, new DataflowLinkOptions() { PropagateCompletion = false }, predicate.GetPredicate(nextBlock, pipelineBulkStage.GetType()));
                    currentBlock.LinkTo(DataflowBlock.NullTarget<PipelineStageItem<TStageOutput>>(), new DataflowLinkOptions() { PropagateCompletion = false });

                }
                else
                {
                    predicate = x => PredicateResult.Keep;
                    currentBlock.LinkTo(targetBlock, new DataflowLinkOptions() { PropagateCompletion = false }, predicate.GetPredicate(nextBlock, pipelineBulkStage.GetType()));
                    currentBlock.LinkTo(DataflowBlock.NullTarget<PipelineStageItem<TStageOutput>>(), new DataflowLinkOptions() { PropagateCompletion = false });
                }

                currentBlock.Completion.ContinueWith(x => targetBlock.Complete());

                return targetBlock;
            }

            return CreatePipelineSetup(CreateExecutionBlock);
        }

        private PipelineSetup<TPipelineInput, TNextStageOutput> CreatePipelineSetup<TNextStageOutput>(Func<StageCreationContext, ISourceBlock<PipelineStageItem<TNextStageOutput>>> executionBlockCreator)
            => CreatePipelineSetup(LinkStageSetupOut(StageSetupOut, CreateStageSetupOut(executionBlockCreator)));
        
        private IStageSetupOut<TNextStageOutput> LinkStageSetupOut<TNextStageOutput>(IStageSetupOut<TStageOutput> source, IStageSetupOut<TNextStageOutput> target)
        {
            target.PreviousStageSetup = source;
            source.NextStageSetup.Add(target);

            return target;
        }

        private PipelineSetup<TPipelineInput, TNextStageOutput> CreatePipelineSetup<TNextStageOutput>(IStageSetupOut<TNextStageOutput> stageSetupOut) 
            => new PipelineSetup<TPipelineInput, TNextStageOutput>(stageSetupOut, PipelineCreationContext);

        private StageSetupOut<TNextStageOutput> CreateStageSetupOut<TNextStageOutput>(Func<StageCreationContext, ISourceBlock<PipelineStageItem<TNextStageOutput>>> executionBlockCreator) 
            => new StageSetupOut<TNextStageOutput>(executionBlockCreator);
    }
}