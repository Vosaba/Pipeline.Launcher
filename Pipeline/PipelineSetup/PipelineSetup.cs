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

        public IPipelineSetup<TInput, TNextOutput> BulkStage<TBulkStage, TNextOutput>(Predicate<TOutput> predicate = null)
            where TBulkStage : BulkStage<TOutput, TNextOutput>
            => CreateNextBulkStage<TNextOutput>(StageService.GetStageInstance<TBulkStage>(), predicate);

        public IPipelineSetup<TInput, TOutput> BulkStage<TBulkStage>(Predicate<TOutput> predicate = null)
            where TBulkStage : BulkStage<TOutput, TOutput>
            => CreateNextBulkStage<TOutput>(StageService.GetStageInstance<TBulkStage>(), predicate);

        #endregion

        #region Stages

        public IPipelineSetup<TInput, TOutput> Stage<TStage>(Predicate<TOutput> predicate = null)
            where TStage : Stages.Stage<TOutput, TOutput>
            => CreateNextStage<TOutput>(StageService.GetStageInstance<TStage>(), predicate);

        public IPipelineSetup<TInput, TNextOutput> Stage<TStage, TNextOutput>(Predicate<TOutput> predicate = null)
            where TStage : Stages.Stage<TOutput, TNextOutput>
            => CreateNextStage<TNextOutput>(StageService.GetStageInstance<TStage>(), predicate);

        #endregion

        #endregion

        #region Nongeneric

        #region BulkStages

        public IPipelineSetup<TInput, TNextOutput> BulkStage<TNextOutput>(BulkStage<TOutput, TNextOutput> bulkStage, Predicate<TOutput> predicate = null)
            => CreateNextBulkStage(bulkStage, predicate);

        public IPipelineSetup<TInput, TNextOutput> BulkStage<TNextOutput>(Func<TOutput[], IEnumerable<TNextOutput>> bulkFunc, BulkStageConfiguration bulkStageConfiguration)
            => BulkStage(new LambdaBulkStage<TOutput, TNextOutput>(bulkFunc, bulkStageConfiguration));

        public IPipelineSetup<TInput, TNextOutput> BulkStage<TNextOutput>(Func<TOutput[], Task<IEnumerable<TNextOutput>>> bulkFunc, BulkStageConfiguration bulkStageConfiguration)
            => BulkStage(new LambdaBulkStage<TOutput, TNextOutput>(bulkFunc, bulkStageConfiguration));

        #endregion

        #region Stages

        public IPipelineSetup<TInput, TNextOutput> Stage<TNextOutput>(Stages.Stage<TOutput, TNextOutput> stage, Predicate<TOutput> predicate = null)
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
            return newCurrent.Branch(branches).RemoveDuplicates(branches.Length);
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
                    });//, Context.CancellationToken);

                    branchId++;
                }

                currentBlock.LinkTo(DataflowBlock.NullTarget<PipelineStageItem<TOutput>>(), new DataflowLinkOptions() { PropagateCompletion = false });


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

        public IPipelineSetup<TInput, TOutput> RemoveDuplicates(int totalOccurrences)
        {
            IPropagatorBlock<PipelineStageItem<TOutput>, PipelineStageItem<TOutput>> MakeNextBlock(StageCreationOptions options)
            {
                var processedHash = new ConcurrentDictionary<object, int>();

                var buffer = new BatchBlock<PipelineStageItem<TOutput>>(1); //TODO
                var context = Context.GetPipelineStageContext(null);


                IEnumerable<PipelineStageItem<TOutput>> Filter(IEnumerable<PipelineStageItem<TOutput>> items)
                {
                    foreach (var item in items)
                    {
                        object originalItem;
                        switch (item)
                        {
                            case NoneResultStageItem<TInput> noneResultItem:
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

                currentBlock.Completion.ContinueWith(x => next.Complete());//, Context.CancellationToken);


                return next;
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

                //currentBlock.GetCompletionTaskFor.ContinueWith(x =>
                //{
                //    nextBlock.RetrieveExecutionBlock(options).Complete();
                //});//, Context.CancellationToken);

                return pipelineSetup.Current.RetrieveExecutionBlock(options);
            };

            var nextStage = new StageSetupOut<TNextOutput>(MakeNextBlock)
            {
                Previous = Current
            };

            Current.Next.Add(nextStage); // Hack with cross linking to destroy

            return new PipelineSetup<TInput, TNextOutput>(nextStage, Context);
        }

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

        private PipelineSetup<TInput, TNextOutput> CreateNextStage<TNextOutput>(PipelineStage<TOutput, TNextOutput> stage, Predicate<TOutput> predicate)
        {
            IPropagatorBlock<PipelineStageItem<TOutput>, PipelineStageItem<TNextOutput>> MakeNextBlock(StageCreationOptions options)
            {

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
                    async x => await stage.BaseExecute(x, Context.GetPipelineStageContext(() => RePostMessage(x))),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = stage.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = stage.Configuration.MaxMessagesPerTask,
                        CancellationToken = Context.CancellationToken,
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

                });//, Context.CancellationToken);

                return nextBlock;
            }

            return CreateNextBlock(MakeNextBlock, stage.Configuration);
        }

        private PipelineSetup<TInput, TNextOutput> CreateNextBulkStage<TNextOutput>(PipelineBulkStage<TOutput, TNextOutput> stage, Predicate<TOutput> predicate)
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
                    async x => await stage.BaseExecute(x, Context.GetPipelineStageContext(() => RePostMessages(x))),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = stage.Configuration.MaxDegreeOfParallelism,
                        MaxMessagesPerTask = stage.Configuration.MaxMessagesPerTask,
                        CancellationToken = Context.CancellationToken,
                        SingleProducerConstrained = stage.Configuration.SingleProducerConstrained,
                        EnsureOrdered = stage.Configuration.EnsureOrdered
                    });

                buffer.LinkTo(nextBlock, new DataflowLinkOptions() { PropagateCompletion = false });
                rePostBlock = nextBlock;

                buffer.Completion.ContinueWith(x =>
                {
                    nextBlock.Complete();
                });//, Context.CancellationToken);

                var next = DataflowBlock.Encapsulate(buffer, nextBlock);
                var currentBlock = Current.RetrieveExecutionBlock(options);

                if (predicate != null || stage is IConditionalStage<TOutput>)
                {
                    if (stage is IConditionalStage<TOutput> stageCondition)
                    {
                        predicate = stageCondition.Predicate;
                    }

                    currentBlock.LinkTo(next, new DataflowLinkOptions() { PropagateCompletion = false }, predicate.GetPredicate(next));
                    currentBlock.LinkTo(DataflowBlock.NullTarget<PipelineStageItem<TOutput>>(), new DataflowLinkOptions() { PropagateCompletion = false });

                }
                else
                {
                    currentBlock.LinkTo(next, new DataflowLinkOptions() { PropagateCompletion = false });
                }

                currentBlock.Completion.ContinueWith(x =>
                {
                    next.Complete();
                });//, Context.CancellationToken);

                return next;
            }

            return CreateNextBlock(MakeNextBlock, stage.Configuration);
        }

        private PipelineSetup<TInput, TNextOutput> CreateNextBlock<TNextOutput>(Func<StageCreationOptions, IPropagatorBlock<PipelineStageItem<TOutput>, PipelineStageItem<TNextOutput>>> nextBlock, StageBaseConfiguration stageConfiguration)
        {
            var nextStage = new StageSetupOut<TNextOutput>(nextBlock)
            {
                Previous = Current,
                PipelineBaseConfiguration = stageConfiguration
            };

            Current.Next.Add(nextStage);

            return new PipelineSetup<TInput, TNextOutput>(nextStage, Context);
        }

    }
}