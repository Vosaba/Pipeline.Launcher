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
    internal abstract class PipelineSetup<TFirstInput> : IPipelineSetup
    {
        protected PipelineCreationContext PipelineCreationContext;
        protected IStageService StageService => PipelineCreationContext.StageService;

        public IStageSetup Current { get; }

        internal PipelineSetup(IStageSetup stageSetup, PipelineCreationContext pipelineCreationContext)
        {
            Current = stageSetup;
            PipelineCreationContext = pipelineCreationContext;
        }
    }

    internal partial class PipelineSetup<TInput, TOutput> : PipelineSetup<TInput>, IPipelineSetup<TInput, TOutput>
    {
        public new IStageSetupOut<TOutput> Current => (IStageSetupOut<TOutput>)base.Current;

        internal PipelineSetup(IStageSetupOut<TOutput> stageSetup, PipelineCreationContext pipelineCreationContext)
            : base(stageSetup, pipelineCreationContext)
        { }

        #region Generic

        #region BulkStages

        public IPipelineSetup<TInput, TNextOutput> BulkStage<TBulkStage, TNextOutput>(PipelinePredicate<TOutput> predicate = null)
            where TBulkStage : BulkStage<TOutput, TNextOutput>
            => CreateNextBulkStage<TNextOutput>(StageService.GetStageInstance<TBulkStage>(), predicate);

        public IPipelineSetup<TInput, TOutput> BulkStage<TBulkStage>(PipelinePredicate<TOutput> predicate = null)
            where TBulkStage : BulkStage<TOutput, TOutput>
            => CreateNextBulkStage<TOutput>(StageService.GetStageInstance<TBulkStage>(), predicate);

        #endregion

        #region Stages

        public IPipelineSetup<TInput, TOutput> Stage<TStage>(PipelinePredicate<TOutput> predicate = null)
            where TStage : Stages.Stage<TOutput, TOutput>
            => CreateNextStage<TOutput>(StageService.GetStageInstance<TStage>(), predicate);

        public IPipelineSetup<TInput, TNextOutput> Stage<TStage, TNextOutput>(PipelinePredicate<TOutput> predicate = null)
            where TStage : Stages.Stage<TOutput, TNextOutput>
            => CreateNextStage<TNextOutput>(StageService.GetStageInstance<TStage>(), predicate);

        #endregion

        #endregion

        #region Nongeneric

        #region BulkStages

        public IPipelineSetup<TInput, TNextOutput> BulkStage<TNextOutput>(BulkStage<TOutput, TNextOutput> bulkStage, PipelinePredicate<TOutput> predicate = null)
            => CreateNextBulkStage(bulkStage, predicate);

        public IPipelineSetup<TInput, TNextOutput> BulkStage<TNextOutput>(Func<TOutput[], IEnumerable<TNextOutput>> bulkFunc, BulkStageConfiguration bulkStageConfiguration)
            => BulkStage(new LambdaBulkStage<TOutput, TNextOutput>(bulkFunc, bulkStageConfiguration));

        public IPipelineSetup<TInput, TNextOutput> BulkStage<TNextOutput>(Func<TOutput[], Task<IEnumerable<TNextOutput>>> bulkFunc, BulkStageConfiguration bulkStageConfiguration)
            => BulkStage(new LambdaBulkStage<TOutput, TNextOutput>(bulkFunc, bulkStageConfiguration));

        #endregion

        #region Stages

        public IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Stages.Stage<TOutput, TNextOutput> stage, PipelinePredicate<TOutput> predicate = null)
            => CreateNextStage(stage, predicate);

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
            BroadcastBlock<PipelineStageItem<TOutput>> MakeNextBlock(StageCreationContext options)
            {
                var broadcastBlock = new BroadcastBlock<PipelineStageItem<TOutput>>(x => x);

                var currentBlock = Current.RetrieveExecutionBlock(options);

                currentBlock.LinkTo(broadcastBlock, new DataflowLinkOptions { PropagateCompletion = false} );
                currentBlock.Completion.ContinueWith(x =>
                {
                    broadcastBlock.Complete();
                });//, PipelineCreationContext.CancellationToken);


                return broadcastBlock;

            }

            var newCurrent = CreateNextBlock(MakeNextBlock);

            //return PipelineSetupExtensions.RemoveDuplicates(newCurrent.Branch(branches));
            return newCurrent.Branch(branches).RemoveDuplicates(branches.Length);
        }

        public IPipelineSetup<TInput, TNextOutput> Branch<TNextOutput>((Predicate<TOutput> predicate,
            Func<IPipelineSetup<TInput, TOutput>, IPipelineSetup<TInput, TNextOutput>> branch)[] branches)
        {
            return Branch(ConditionExceptionScenario.GoToNextCondition, branches);
        }

        public IPipelineSetup<TInput, TNextOutput> Branch<TNextOutput>(ConditionExceptionScenario conditionExceptionScenario, (Predicate<TOutput> predicate, Func<IPipelineSetup<TInput, TOutput>, IPipelineSetup<TInput, TNextOutput>> branch)[] branches)
        {
            IPropagatorBlock<PipelineStageItem<TNextOutput>, PipelineStageItem<TNextOutput>> MakeNextBlock(StageCreationContext options)
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

                    var nextBlockAfterCurrent = new PipelineSetup<TInput, TOutput>(newtBranchStageHead, PipelineCreationContext);

                    var newBranch = branch.branch(nextBlockAfterCurrent);

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
                                        mergeBlock.Post(new ExceptionStageItem<TNextOutput>(ex, null, branch.predicate.GetType(), x.Item));
                                        return false;
                                    case ConditionExceptionScenario.StopPipelineExecution:
                                    default:
                                        throw;
                                }
                            }
                        });


                    var newBranchBlock = newBranch.Current.RetrieveExecutionBlock(options);

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

                currentBlock.LinkTo(DataflowBlock.NullTarget<PipelineStageItem<TOutput>>(), new DataflowLinkOptions() { PropagateCompletion = false });


                currentBlock.Completion.ContinueWith(x =>
                {
                    foreach (var headBranch in headBranches)
                    {
                        headBranch.Complete();
                    }
                });//, PipelineCreationContext.CancellationToken);

                return mergeBlock;
            };

            var nextStage = new StageSetupOut<TNextOutput>(MakeNextBlock)//TODO
            {
                Previous = Current
            };

            Current.Next.Add(nextStage); // Hack with cross linking to destroy
            return new PipelineSetup<TInput, TNextOutput>(nextStage, PipelineCreationContext);
        }

        #endregion

        public IPipelineSetup<TInput, TOutput> RemoveDuplicates(int totalOccurrences)
        {
            IPropagatorBlock<PipelineStageItem<TOutput>, PipelineStageItem<TOutput>> MakeNextBlock(StageCreationContext options)
            {
                var processedHash = new ConcurrentDictionary<object, int>();

                var buffer = new BatchBlock<PipelineStageItem<TOutput>>(1); //TODO
                //var pipelineCreationContext = PipelineCreationContext.GetPipelineStageContext(null);


                IEnumerable<PipelineStageItem<TOutput>> Filter(IEnumerable<PipelineStageItem<TOutput>> items)
                {
                    foreach (var item in items)
                    {
                        object originalItem;
                        switch (item)
                        {
                            case NonResultStageItem<TInput> noneResultItem:
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

                var mergeBlock = new TransformManyBlock<IEnumerable<PipelineStageItem<TOutput>>, PipelineStageItem<TOutput>>(Filter);
               
                buffer.LinkTo(mergeBlock);
                buffer.Completion.ContinueWith(x => mergeBlock.Complete());

                var next = DataflowBlock.Encapsulate(buffer, mergeBlock);

                var currentBlock = Current.RetrieveExecutionBlock(options);

                currentBlock.LinkTo(next);

                currentBlock.Completion.ContinueWith(x => next.Complete());//, PipelineCreationContext.CancellationToken);


                return next;
            }

            return CreateNextBlock(MakeNextBlock);
        }

        public IPipelineSetup<TInput, TNextOutput> MergeWith<TNextOutput>(IPipelineSetup<TOutput, TNextOutput> pipelineSetup)
        {
            var nextBlock = pipelineSetup.GetFirstStage<TOutput>();

            ISourceBlock<PipelineStageItem<TNextOutput>> MakeNextBlock(StageCreationContext options)
            {
                Current.Next.Add(nextBlock);
                nextBlock.Previous = Current;
                var currentBlock = Current.RetrieveExecutionBlock(options);

                currentBlock.LinkTo(nextBlock.RetrieveExecutionBlock(options), new DataflowLinkOptions() { PropagateCompletion = false });
                currentBlock.Completion.ContinueWith(x =>
                {
                    nextBlock.RetrieveExecutionBlock(options).Complete();
                });//, PipelineCreationContext.CancellationToken);

                //currentBlock.GetCompletionTaskFor.ContinueWith(x =>
                //{
                //    nextBlock.RetrieveExecutionBlock(pipelineCreationContext).Complete();
                //});//, PipelineCreationContext.CancellationToken);

                return pipelineSetup.Current.RetrieveExecutionBlock(options);
            };

            var nextStage = new StageSetupOut<TNextOutput>(MakeNextBlock)
            {
                Previous = Current
            };

            Current.Next.Add(nextStage); // Hack with cross linking to destroy

            return new PipelineSetup<TInput, TNextOutput>(nextStage, PipelineCreationContext);
        }

        public IAwaitablePipelineRunner<TInput, TOutput> CreateAwaitable(AwaitablePipelineCreationConfig pipelineCreationConfig = null)
        {
            if (pipelineCreationConfig == null)
            {
                pipelineCreationConfig = new AwaitablePipelineCreationConfig();
            }

            var firstStage = this.GetFirstStage<TInput>();
            return new AwaitablePipelineRunner<TInput, TOutput>(firstStage.RetrieveExecutionBlock, Current.RetrieveExecutionBlock, () => firstStage.DestroyStageBlocks(), pipelineCreationConfig);
        }

        public IPipelineRunner<TInput, TOutput> Create(PipelineCreationConfig pipelineCreationConfig = null)
        {
            if (pipelineCreationConfig == null)
            {
                pipelineCreationConfig = new PipelineCreationConfig();
            }

            var firstStage = this.GetFirstStage<TInput>();
            return new PipelineRunner<TInput, TOutput>(firstStage.RetrieveExecutionBlock, Current.RetrieveExecutionBlock, pipelineCreationConfig);
        }

        private PipelineSetup<TInput, TNextOutput> CreateNextStage<TNextOutput>(IPipelineStage<TOutput, TNextOutput> stageA, PipelinePredicate<TOutput> predicate)
        {
            IPropagatorBlock<PipelineStageItem<TOutput>, PipelineStageItem<TNextOutput>> MakeNextBlock(StageCreationContext options)
            {
                var stage = new PipelineStage<TOutput, TNextOutput>(stageA);

                TransformBlock<PipelineStageItem<TOutput>, PipelineStageItem<TNextOutput>> rePostBlock = null;

                Action<PipelineStageItem<TOutput>> RePostMessage;
                if (options.PipelineType == PipelineType.Normal)
                {
                    RePostMessage = message => rePostBlock?.Post(message);
                }
                else
                {
                    RePostMessage = message => throw new PipelineUsageException(Helpers.Strings.RetryOnAwaitable);
                }

                var nextBlock = new TransformBlock<PipelineStageItem<TOutput>, PipelineStageItem<TNextOutput>>(
                    async x => await stage.BaseExecute(x, options.GetPipelineStageContext(() => RePostMessage(x))),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = stage.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = stage.Configuration.MaxMessagesPerTask,
                        CancellationToken = options.CancellationToken,
                        SingleProducerConstrained = stage.Configuration.SingleProducerConstrained,
                        EnsureOrdered = stage.Configuration.EnsureOrdered
                    });

                rePostBlock = nextBlock;
                var currentBlock = Current.RetrieveExecutionBlock(options);

                if (predicate != null || stage is IConditionalStage<TOutput>)
                {
                    if(stage is IConditionalStage<TOutput> stageCondition)
                    {
                        predicate = stageCondition.Predicate;
                    }

                    currentBlock.LinkTo(nextBlock, new DataflowLinkOptions() { PropagateCompletion = false }, predicate.GetPredicate(nextBlock));

                    currentBlock.LinkTo(DataflowBlock.NullTarget<PipelineStageItem<TOutput>>(), new DataflowLinkOptions() { PropagateCompletion = false });
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

        private PipelineSetup<TInput, TNextOutput> CreateNextBulkStage<TNextOutput>(IPipelineBulkStage<TOutput, TNextOutput> stageA, PipelinePredicate<TOutput> predicate)
        {
            IPropagatorBlock<PipelineStageItem<TOutput>, PipelineStageItem<TNextOutput>> MakeNextBlock(StageCreationContext options)
            {
                var stage = new PipelineBulkStage<TOutput, TNextOutput>(stageA);

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

                Action<IEnumerable<PipelineStageItem<TOutput>>> RePostMessages;
                if (options.PipelineType == PipelineType.Normal)
                {
                    RePostMessages = messages => rePostBlock?.Post(messages);
                }
                else
                {
                    RePostMessages = messages => throw new PipelineUsageException(Helpers.Strings.RetryOnAwaitable);
                }

                var nextBlock = new TransformManyBlock<IEnumerable<PipelineStageItem<TOutput>>, PipelineStageItem<TNextOutput>>(
                    async x => await stage.BaseExecute(x, options.GetPipelineStageContext(() => RePostMessages(x))),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = stage.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = stage.Configuration.MaxMessagesPerTask,
                        CancellationToken = options.CancellationToken,
                        SingleProducerConstrained = stage.Configuration.SingleProducerConstrained,
                        EnsureOrdered = stage.Configuration.EnsureOrdered
                    });

                buffer.LinkTo(nextBlock, new DataflowLinkOptions() { PropagateCompletion = false });
                rePostBlock = nextBlock;

                buffer.Completion.ContinueWith(x =>
                {
                    nextBlock.Complete();
                });//, PipelineCreationContext.CancellationToken);

                var next = DataflowBlock.Encapsulate(buffer, nextBlock);
                var currentBlock = Current.RetrieveExecutionBlock(options);

                if (predicate != null || stage is IConditionalStage<TOutput>)
                {
                    if (stage is IConditionalStage<TOutput> stageCondition)
                    {
                        predicate = stageCondition.Predicate;
                    }

                    currentBlock.LinkTo(next, new DataflowLinkOptions() { PropagateCompletion = false }, predicate.GetPredicate(nextBlock, stage.GetType()));
                    currentBlock.LinkTo(DataflowBlock.NullTarget<PipelineStageItem<TOutput>>(), new DataflowLinkOptions() { PropagateCompletion = false });

                }
                else
                {
                    predicate = x => PredicateResult.Keep;
                    currentBlock.LinkTo(next, new DataflowLinkOptions() { PropagateCompletion = false }, predicate.GetPredicate(nextBlock, stage.GetType()));
                    currentBlock.LinkTo(DataflowBlock.NullTarget<PipelineStageItem<TOutput>>(), new DataflowLinkOptions() { PropagateCompletion = false });
                }

                currentBlock.Completion.ContinueWith(x =>
                {
                    next.Complete();
                });//, PipelineCreationContext.CancellationToken);

                return next;
            }

            return CreateNextBlock(MakeNextBlock);
        }

        private PipelineSetup<TInput, TNextOutput> CreateNextBlock<TNextOutput>(Func<StageCreationContext, IPropagatorBlock<PipelineStageItem<TOutput>, PipelineStageItem<TNextOutput>>> nextBlock)
        {
            var nextStage = new StageSetupOut<TNextOutput>(nextBlock)
            {
                Previous = Current,
            };

            Current.Next.Add(nextStage);

            return new PipelineSetup<TInput, TNextOutput>(nextStage, PipelineCreationContext);
        }

    }
}