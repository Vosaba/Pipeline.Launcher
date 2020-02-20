using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.Blocks;
using System;
using System.Collections;
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
using PipelineLauncher.Exceptions;
using PipelineLauncher.Abstractions.Stages;

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

        internal PipelineSetup(
            IStageSetupOut<TStageOutput> stageSetupOut,
            PipelineCreationContext pipelineCreationContext)
            : base(stageSetupOut, pipelineCreationContext)
        { }

        #region Generic

        #region BulkStages

        public IPipelineSetup<TPipelineInput, TNextStageOutput> BulkStage<TBulkStage, TNextStageOutput>(PipelinePredicate<TStageOutput> predicate = null)
            where TBulkStage : BulkStage<TStageOutput, TNextStageOutput>
            => CreateNextBulkStage(StageService.GetStageInstance<TBulkStage>(), predicate);

        public IPipelineSetup<TPipelineInput, TStageOutput> BulkStage<TBulkStage>(PipelinePredicate<TStageOutput> predicate = null)
            where TBulkStage : BulkStage<TStageOutput, TStageOutput>
            => CreateNextBulkStage(StageService.GetStageInstance<TBulkStage>(), predicate);

        #endregion

        #region Stages

        public IPipelineSetup<TPipelineInput, TStageOutput> Stage<TStage>(PipelinePredicate<TStageOutput> predicate = null)
            where TStage : Stage<TStageOutput, TStageOutput>
            => CreateNextStage(StageService.GetStageInstance<TStage>(), predicate);

        public IPipelineSetup<TPipelineInput, TNextStageOutput> Stage<TStage, TNextStageOutput>(PipelinePredicate<TStageOutput> predicate = null)
            where TStage : Stage<TStageOutput, TNextStageOutput>
            => CreateNextStage(StageService.GetStageInstance<TStage>(), predicate);

        #endregion

        #endregion

        #region Nongeneric

        #region BulkStages

        public IPipelineSetup<TPipelineInput, TNextStageOutput> BulkStage<TNextStageOutput>(BulkStage<TStageOutput, TNextStageOutput> bulkStage, PipelinePredicate<TStageOutput> predicate = null)
            => CreateNextBulkStage(bulkStage, predicate);

        public IPipelineSetup<TPipelineInput, TNextStageOutput> BulkStage<TNextStageOutput>(Func<TStageOutput[], IEnumerable<TNextStageOutput>> func, BulkStageConfiguration bulkStageConfiguration)
            => BulkStage(new LambdaBulkStage<TStageOutput, TNextStageOutput>(func, bulkStageConfiguration));

        public IPipelineSetup<TPipelineInput, TNextStageOutput> BulkStage<TNextStageOutput>(Func<TStageOutput[], Task<IEnumerable<TNextStageOutput>>> func, BulkStageConfiguration bulkStageConfiguration)
            => BulkStage(new LambdaBulkStage<TStageOutput, TNextStageOutput>(func, bulkStageConfiguration));

        #endregion

        #region Stages

        public IPipelineSetup<TPipelineInput, TNextStageOutput> Stage<TNextStageOutput>(Stage<TStageOutput, TNextStageOutput> stage, PipelinePredicate<TStageOutput> predicate = null)
            => CreateNextStage(stage, predicate);

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
                Func<IPipelineSetup<TPipelineInput, TStageOutput>, IPipelineSetup<TPipelineInput, TNextStageOutput>> branchSetup)[] branches)
        {
            BroadcastBlock<PipelineStageItem<TStageOutput>> MakeNextBlock(StageCreationContext options)
            {
                var broadcastBlock = new BroadcastBlock<PipelineStageItem<TStageOutput>>(x => x);

                var currentBlock = StageSetupOut.RetrieveExecutionBlock(options);

                currentBlock.LinkTo(broadcastBlock, new DataflowLinkOptions { PropagateCompletion = false} );
                currentBlock.Completion.ContinueWith(x =>
                {
                    broadcastBlock.Complete();
                });//, PipelineCreationContext.CancellationToken);


                return broadcastBlock;
            }

            var newCurrent = CreateNextBlock(MakeNextBlock);

            return newCurrent.Branch(branches).RemoveDuplicates(branches.Length);
        }

        public IPipelineSetup<TPipelineInput, TNextStageOutput> Branch<TNextStageOutput>(
            (Predicate<TStageOutput> predicate, 
                Func<IPipelineSetup<TPipelineInput, TStageOutput>, IPipelineSetup<TPipelineInput, TNextStageOutput>> branchSetup)[] branches)
        {
            return Branch(ConditionExceptionScenario.GoToNextCondition, branches);
        }

        public IPipelineSetup<TPipelineInput, TNextStageOutput> Branch<TNextStageOutput>(
            ConditionExceptionScenario conditionExceptionScenario,
            (Predicate<TStageOutput> predicate, 
                Func<IPipelineSetup<TPipelineInput, TStageOutput>, IPipelineSetup<TPipelineInput, TNextStageOutput>> branchSetup)[] branches)
        {
            IPropagatorBlock<PipelineStageItem<TNextStageOutput>, PipelineStageItem<TNextStageOutput>> MakeNextBlock(StageCreationContext options)
            {
                var mergeBlock = new TransformBlock<PipelineStageItem<TNextStageOutput>, PipelineStageItem<TNextStageOutput>>(x => x);

                IDataflowBlock[] headBranches = new IDataflowBlock[branches.Length];
                IDataflowBlock[] tailBranches = new IDataflowBlock[branches.Length];

                var branchId = 0;

                var currentBlock = StageSetupOut.RetrieveExecutionBlock(options);
                foreach (var branch in branches)
                {
                    var newBranchHead = new TransformBlock<PipelineStageItem<TStageOutput>, PipelineStageItem<TStageOutput>>(x => x);

                    headBranches[branchId] = newBranchHead;

                    var newtBranchStageHead = new StageSetupOut<TStageOutput>((d) => newBranchHead)
                    {
                        PreviousStageSetup = StageSetupOut
                    };

                    StageSetupOut.NextStageSetup.Add(newtBranchStageHead);

                    var nextBlockAfterCurrent = new PipelineSetup<TPipelineInput, TStageOutput>(newtBranchStageHead, PipelineCreationContext);

                    var newBranch = branch.branchSetup(nextBlockAfterCurrent);

                    currentBlock.LinkTo(newBranchHead, new DataflowLinkOptions { PropagateCompletion = false },
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
                                return branch.predicate(x.Item);
                            }
                            catch (Exception ex)
                            {
                                switch (conditionExceptionScenario)
                                {
                                    case ConditionExceptionScenario.GoToNextCondition:
                                        return false;
                                    case ConditionExceptionScenario.AddExceptionAndGoToNextCondition:
                                        mergeBlock.Post(new ExceptionStageItem<TNextStageOutput>(ex, null, branch.predicate.GetType(), x.Item));
                                        return false;
                                    case ConditionExceptionScenario.StopPipelineExecution:
                                    default:
                                        throw;
                                }
                            }
                        });


                    var newBranchBlock = newBranch.StageSetupOut.RetrieveExecutionBlock(options);

                    tailBranches[branchId] = newBranchBlock;

                    newBranchBlock.LinkTo(mergeBlock, new DataflowLinkOptions { PropagateCompletion = false } ); //TODO broadcast TEST 

                    newBranchBlock.Completion.ContinueWith(x =>
                    {
                        if (tailBranches.All(tail => tail.Completion.IsCompleted))
                        {
                            mergeBlock.Complete();
                        }
                    });//, PipelineCreationContext.CancellationToken);

                    branchId++;
                }

                currentBlock.LinkTo(DataflowBlock.NullTarget<PipelineStageItem<TStageOutput>>(), new DataflowLinkOptions() { PropagateCompletion = false });


                currentBlock.Completion.ContinueWith(x =>
                {
                    foreach (var headBranch in headBranches)
                    {
                        headBranch.Complete();
                    }
                });//, PipelineCreationContext.CancellationToken);

                return mergeBlock;
            };

            var nextStage = new StageSetupOut<TNextStageOutput>(MakeNextBlock)//TODO
            {
                PreviousStageSetup = StageSetupOut
            };

            StageSetupOut.NextStageSetup.Add(nextStage); // Hack with cross linking to destroy
            return new PipelineSetup<TPipelineInput, TNextStageOutput>(nextStage, PipelineCreationContext);
        }

        #endregion

        public IPipelineSetup<TPipelineInput, TStageOutput> RemoveDuplicates(int totalOccurrences)
        {
            IPropagatorBlock<PipelineStageItem<TStageOutput>, PipelineStageItem<TStageOutput>> MakeNextBlock(StageCreationContext options)
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

                var currentBlock = StageSetupOut.RetrieveExecutionBlock(options);

                currentBlock.LinkTo(next);

                currentBlock.Completion.ContinueWith(x => next.Complete());//, PipelineCreationContext.CancellationToken);


                return next;
            }

            return CreateNextBlock(MakeNextBlock);
        }

        public IPipelineSetup<TPipelineInput, TNextStageOutput> MergeWith<TNextStageOutput>(IPipelineSetup<TStageOutput, TNextStageOutput> pipelineSetup)
        {
            var nextBlock = pipelineSetup.GetFirstStage<TStageOutput>();

            ISourceBlock<PipelineStageItem<TNextStageOutput>> MakeNextBlock(StageCreationContext options)
            {
                StageSetupOut.NextStageSetup.Add(nextBlock);
                nextBlock.PreviousStageSetup = StageSetupOut;
                var currentBlock = StageSetupOut.RetrieveExecutionBlock(options);

                currentBlock.LinkTo(nextBlock.RetrieveExecutionBlock(options), new DataflowLinkOptions() { PropagateCompletion = false });
                currentBlock.Completion.ContinueWith(x =>
                {
                    nextBlock.RetrieveExecutionBlock(options).Complete();
                });//, PipelineCreationContext.CancellationToken);

                //currentBlock.GetCompletionTaskFor.ContinueWith(x =>
                //{
                //    nextBlock.RetrieveExecutionBlock(pipelineCreationContext).Complete();
                //});//, PipelineCreationContext.CancellationToken);

                return pipelineSetup.StageSetupOut.RetrieveExecutionBlock(options);
            };

            var nextStage = new StageSetupOut<TNextStageOutput>(MakeNextBlock)
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

        private PipelineSetup<TPipelineInput, TNextStageOutput> CreateNextStage<TNextStageOutput>(IStage<TStageOutput, TNextStageOutput> stage, PipelinePredicate<TStageOutput> predicate)
        {
            IPropagatorBlock<PipelineStageItem<TStageOutput>, PipelineStageItem<TNextStageOutput>> MakeNextBlock(StageCreationContext stageCreationContext)
            {
                var pipelineStage = new PipelineStage<TStageOutput, TNextStageOutput>(stage);

                TransformBlock<PipelineStageItem<TStageOutput>, PipelineStageItem<TNextStageOutput>> rePostBlock = null;

                Action<PipelineStageItem<TStageOutput>> RePostMessage;
                if (stageCreationContext.PipelineType == PipelineType.Normal)
                {
                    RePostMessage = message => rePostBlock?.Post(message);
                }
                else
                {
                    RePostMessage = message => throw new PipelineUsageException(Helpers.Strings.RetryOnAwaitable);
                }

                var nextBlock = new TransformBlock<PipelineStageItem<TStageOutput>, PipelineStageItem<TNextStageOutput>>(
                    async x => await pipelineStage.BaseExecute(x, stageCreationContext.GetPipelineStageContext(() => RePostMessage(x))),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = pipelineStage.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = pipelineStage.Configuration.MaxMessagesPerTask,
                        CancellationToken = stageCreationContext.CancellationToken,
                        SingleProducerConstrained = pipelineStage.Configuration.SingleProducerConstrained,
                        EnsureOrdered = pipelineStage.Configuration.EnsureOrdered
                    });

                rePostBlock = nextBlock;
                var currentBlock = StageSetupOut.RetrieveExecutionBlock(stageCreationContext);

                if (predicate != null || pipelineStage is IConditionalStage<TStageOutput>)
                {
                    if(pipelineStage is IConditionalStage<TStageOutput> stageCondition)
                    {
                        predicate = stageCondition.Predicate;
                    }

                    currentBlock.LinkTo(nextBlock, new DataflowLinkOptions() { PropagateCompletion = false }, predicate.GetPredicate(nextBlock));

                    currentBlock.LinkTo(DataflowBlock.NullTarget<PipelineStageItem<TStageOutput>>(), new DataflowLinkOptions() { PropagateCompletion = false });
                }
                else
                {
                    currentBlock.LinkTo(nextBlock, new DataflowLinkOptions() { PropagateCompletion = false });
                }

                currentBlock.Completion.ContinueWith(x =>
                {
                    //DiagnosticHandler?.Invoke(new DiagnosticItem)

                        nextBlock.Complete();

                });//, PipelineCreationContext.CancellationToken);

                return nextBlock;
            }

            return CreateNextBlock(MakeNextBlock);
        }

        private PipelineSetup<TPipelineInput, TNextStageOutput> CreateNextBulkStage<TNextStageOutput>(IBulkStage<TStageOutput, TNextStageOutput> bulkStage, PipelinePredicate<TStageOutput> predicate)
        {
            IPropagatorBlock<PipelineStageItem<TStageOutput>, PipelineStageItem<TNextStageOutput>> MakeNextBlock(StageCreationContext stageCreationContext)
            {
                var pipelineBulkStage = new PipelineBulkStage<TStageOutput, TNextStageOutput>(bulkStage);

                IPropagatorBlock<PipelineStageItem<TStageOutput>, PipelineStageItem<TStageOutput>[]> buffer;
                if (stageCreationContext.UseTimeOuts)
                {
                    buffer = new BatchBlockEx<PipelineStageItem<TStageOutput>>(pipelineBulkStage.Configuration.BatchItemsCount, pipelineBulkStage.Configuration.BatchItemsTimeOut); //TODO
                }
                else
                {
                    buffer = new BatchBlock<PipelineStageItem<TStageOutput>>(pipelineBulkStage.Configuration.BatchItemsCount); //TODO
                }

                TransformManyBlock<IEnumerable<PipelineStageItem<TStageOutput>>, PipelineStageItem<TNextStageOutput>> rePostBlock = null;

                Action<IEnumerable<PipelineStageItem<TStageOutput>>> RePostMessages;
                if (stageCreationContext.PipelineType == PipelineType.Normal)
                {
                    RePostMessages = messages => rePostBlock?.Post(messages);
                }
                else
                {
                    RePostMessages = messages => throw new PipelineUsageException(Helpers.Strings.RetryOnAwaitable);
                }

                var nextBlock = new TransformManyBlock<IEnumerable<PipelineStageItem<TStageOutput>>, PipelineStageItem<TNextStageOutput>>(
                    async x => await pipelineBulkStage.BaseExecute(x, stageCreationContext.GetPipelineStageContext(() => RePostMessages(x))),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = pipelineBulkStage.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = pipelineBulkStage.Configuration.MaxMessagesPerTask,
                        CancellationToken = stageCreationContext.CancellationToken,
                        SingleProducerConstrained = pipelineBulkStage.Configuration.SingleProducerConstrained,
                        EnsureOrdered = pipelineBulkStage.Configuration.EnsureOrdered
                    });

                buffer.LinkTo(nextBlock, new DataflowLinkOptions() { PropagateCompletion = false });
                rePostBlock = nextBlock;

                buffer.Completion.ContinueWith(x =>
                {
                    nextBlock.Complete();
                });//, PipelineCreationContext.CancellationToken);

                var next = DataflowBlock.Encapsulate(buffer, nextBlock);
                var currentBlock = StageSetupOut.RetrieveExecutionBlock(stageCreationContext);

                if (predicate != null || pipelineBulkStage is IConditionalStage<TStageOutput>)
                {
                    if (pipelineBulkStage is IConditionalStage<TStageOutput> stageCondition)
                    {
                        predicate = stageCondition.Predicate;
                    }

                    currentBlock.LinkTo(next, new DataflowLinkOptions() { PropagateCompletion = false }, predicate.GetPredicate(nextBlock, pipelineBulkStage.GetType()));
                    currentBlock.LinkTo(DataflowBlock.NullTarget<PipelineStageItem<TStageOutput>>(), new DataflowLinkOptions() { PropagateCompletion = false });

                }
                else
                {
                    predicate = x => PredicateResult.Keep;
                    currentBlock.LinkTo(next, new DataflowLinkOptions() { PropagateCompletion = false }, predicate.GetPredicate(nextBlock, pipelineBulkStage.GetType()));
                    currentBlock.LinkTo(DataflowBlock.NullTarget<PipelineStageItem<TStageOutput>>(), new DataflowLinkOptions() { PropagateCompletion = false });
                }

                currentBlock.Completion.ContinueWith(x =>
                {
                    next.Complete();
                });//, PipelineCreationContext.CancellationToken);

                return next;
            }

            return CreateNextBlock(MakeNextBlock);
        }

        private PipelineSetup<TPipelineInput, TNextStageOutput> CreateNextBlock<TNextStageOutput>(Func<StageCreationContext, IPropagatorBlock<PipelineStageItem<TStageOutput>, PipelineStageItem<TNextStageOutput>>> nextBlock)
        {
            var stageSetupOut = new StageSetupOut<TNextStageOutput>(nextBlock)
            {
                PreviousStageSetup = StageSetupOut,
            };

            StageSetupOut.NextStageSetup.Add(stageSetupOut);

            return new PipelineSetup<TPipelineInput, TNextStageOutput>(stageSetupOut, PipelineCreationContext);
        }

    }
}