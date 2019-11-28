using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.Blocks;
using PipelineLauncher.Jobs;
using PipelineLauncher.PipelineJobs;
using PipelineLauncher.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Pipeline;

namespace PipelineLauncher.Stages
{
    public abstract class StageSetup<TFirstInput> : IStageSetup
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

        internal StageSetup(IStage stage, IJobService jobService)
        {
            Current = stage;
            JobService = jobService;
        }
    }

    public class StageSetupIn<TFirstInput, TInput> : StageSetup<TFirstInput>, IStageSetupIn<TInput>
    {
        public IStageIn<TInput> Current => (IStageIn<TInput>)base.Current;

        internal StageSetupIn(IStageIn<TInput> stage, IJobService jobService)
            : base(stage, jobService)
        { }
    }

    public class StageSetupOut<TFirstInput, TOutput> : StageSetup<TFirstInput>, IStageSetupOut<TOutput>
    {
        public IStageOut<TOutput> Current => (IStageOut<TOutput>)base.Current;

        internal StageSetupOut(IStageOut<TOutput> stage, IJobService jobService)
            : base(stage, jobService)
        { }

        #region Generic Stages

        public StageSetupOut<TFirstInput, TNexTOutput> Stage<TJob, TNexTOutput>()
            where TJob : Job<TOutput, TNexTOutput>
            => CreateNextStage<TNexTOutput>(GetJobService.GetJobInstance<TJob>());

        public StageSetupOut<TFirstInput, TOutput> Stage<TJob>()
            where TJob : Job<TOutput, TOutput>
            => CreateNextStage<TOutput>(GetJobService.GetJobInstance<TJob>());

        public StageSetupOut<TFirstInput, TNexTOutput> Stage<TJob, TJob2, TNexTOutput>()
            where TJob : Job<TOutput, TNexTOutput>
            where TJob2 : Job<TOutput, TNexTOutput>
            => Stage(GetJobService.GetJobInstance<TJob>(), GetJobService.GetJobInstance<TJob2>());

        public StageSetupOut<TFirstInput, TNexTOutput> Stage<TJob, TJob2, TJob3, TNexTOutput>()
            where TJob : Job<TOutput, TNexTOutput>
            where TJob2 : Job<TOutput, TNexTOutput>
            where TJob3 : Job<TOutput, TNexTOutput>
            => Stage(GetJobService.GetJobInstance<TJob>(), GetJobService.GetJobInstance<TJob2>(), GetJobService.GetJobInstance<TJob3>());

        public StageSetupOut<TFirstInput, TNexTOutput> Stage<TJob, TJob2, TJob3, TJob4, TNexTOutput>()
            where TJob : Job<TOutput, TNexTOutput>
            where TJob2 : Job<TOutput, TNexTOutput>
            where TJob3 : Job<TOutput, TNexTOutput>
            where TJob4 : Job<TOutput, TNexTOutput>
            => Stage(GetJobService.GetJobInstance<TJob>(), GetJobService.GetJobInstance<TJob2>(), GetJobService.GetJobInstance<TJob3>(), GetJobService.GetJobInstance<TJob4>());

        public StageSetupOut<TFirstInput, TOutput> AsyncStage<TAsyncJob>()
            where TAsyncJob : AsyncJob<TOutput, TOutput>
            => CreateNextStageAsync<TOutput>(GetJobService.GetJobInstance<TAsyncJob>());

        public StageSetupOut<TFirstInput, TNexTOutput> AsyncStage<TAsyncJob, TNexTOutput>()
            where TAsyncJob : AsyncJob<TOutput, TNexTOutput>
            => CreateNextStageAsync<TNexTOutput>(GetJobService.GetJobInstance<TAsyncJob>());

        public StageSetupOut<TFirstInput, TNexTOutput> AsyncStage<TAsyncJob, TAsyncJob2, TNexTOutput>()
            where TAsyncJob : AsyncJob<TOutput, TNexTOutput>
            where TAsyncJob2 : AsyncJob<TOutput, TNexTOutput>
            => AsyncStage(GetJobService.GetJobInstance<TAsyncJob>(), GetJobService.GetJobInstance<TAsyncJob2>());

        public StageSetupOut<TFirstInput, TNexTOutput> AsyncStage<TAsyncJob, TAsyncJob2, TAsyncJob3, TNexTOutput>()
            where TAsyncJob : AsyncJob<TOutput, TNexTOutput>
            where TAsyncJob2 : AsyncJob<TOutput, TNexTOutput>
            where TAsyncJob3 : AsyncJob<TOutput, TNexTOutput>
            => AsyncStage(GetJobService.GetJobInstance<TAsyncJob>(), GetJobService.GetJobInstance<TAsyncJob2>(), GetJobService.GetJobInstance<TAsyncJob3>());

        public StageSetupOut<TFirstInput, TNexTOutput> AsyncStage<TAsyncJob, TAsyncJob2, TAsyncJob3, TAsyncJob4, TNexTOutput>()
            where TAsyncJob : AsyncJob<TOutput, TNexTOutput>
            where TAsyncJob2 : AsyncJob<TOutput, TNexTOutput>
            where TAsyncJob3 : AsyncJob<TOutput, TNexTOutput>
            where TAsyncJob4 : AsyncJob<TOutput, TNexTOutput>
            => AsyncStage(GetJobService.GetJobInstance<TAsyncJob>(), GetJobService.GetJobInstance<TAsyncJob2>(), GetJobService.GetJobInstance<TAsyncJob3>(), GetJobService.GetJobInstance<TAsyncJob4>());

        public StageSetupOut<TFirstInput, TNexTOutput> MapAs<TNexTOutput>()
            where TNexTOutput : class
            => AsyncStage(output => output as TNexTOutput);

        #endregion

        #region Nongeneric Stages

        public StageSetupOut<TFirstInput, TNexTOutput> Stage<TNexTOutput>(Job<TOutput, TNexTOutput> job)
            => CreateNextStage<TNexTOutput>(job);

        public StageSetupOut<TFirstInput, TNexTOutput> Stage<TNexTOutput>(Func<IEnumerable<TOutput>, IEnumerable<TNexTOutput>> func)
            => Stage(new LambdaJob<TOutput, TNexTOutput>(func));

        public StageSetupOut<TFirstInput, TNexTOutput> Stage<TNexTOutput>(params Job<TOutput, TNexTOutput>[] jobs)
            => Stage(new ConditionJob<TOutput, TNexTOutput>(jobs));

        public StageSetupOut<TFirstInput, TNexTOutput> AsyncStage<TNexTOutput>(AsyncJob<TOutput, TNexTOutput> asyncJob)
            => CreateNextStageAsync<TNexTOutput>(asyncJob);

        public StageSetupOut<TFirstInput, TNexTOutput> AsyncStage<TNexTOutput>(Func<TOutput, TNexTOutput> func)
            => AsyncStage(new AsyncLambdaJob<TOutput, TNexTOutput>(func));

        public StageSetupOut<TFirstInput, TNexTOutput> AsyncStage<TNexTOutput>(params AsyncJob<TOutput, TNexTOutput>[] asyncJobs)
            => AsyncStage(new ConditionAsyncJob<TOutput, TNexTOutput>(asyncJobs));

        #endregion

        #region Nongeneric Branch

        public StageSetupOut<TFirstInput, TNexTOutput> Broadcast<TNexTOutput>(params (Predicate<TOutput> predicate,
            Func<StageSetupOut<TFirstInput, TOutput>, StageSetupOut<TFirstInput, TNexTOutput>> branch)[] branches)
        {
            var newCurrentBlock = new BroadcastBlock<PipelineItem<TOutput>>(e => e);
            var newCurrent = CreateNextBlock(newCurrentBlock);
            return newCurrent.Branch(branches);
        }

        public StageSetupOut<TFirstInput, TNexTOutput> Branch<TNexTOutput>(params (Predicate<TOutput> predicate,
                Func<StageSetupOut<TFirstInput, TOutput>, StageSetupOut<TFirstInput, TNexTOutput>> branch)[] branches)
        {
            var mergeBlock = new TransformBlock<PipelineItem<TNexTOutput>, PipelineItem<TNexTOutput>>(e => e);
            

            IDataflowBlock[] headBranches = new IDataflowBlock[branches.Length];
            IDataflowBlock[] tailBranches = new IDataflowBlock[branches.Length];

            var branchId = 0;
            foreach (var branch in branches)
            {
                var newBranchHead = new TransformBlock<PipelineItem<TOutput>, PipelineItem<TOutput>>(e => e);

                headBranches[branchId] = newBranchHead;

                var newtBranchStageHead = new StageOut<TOutput>(newBranchHead, Current.CancellationToken)
                {
                    Previous = Current
                };
                Current.Next.Add(newtBranchStageHead);

                var nextBlockAfterCurrent = new StageSetupOut<TFirstInput, TOutput>(newtBranchStageHead, JobService);
                var newBranch = branch.branch(nextBlockAfterCurrent);

                Current.ExecutionBlock.LinkTo(newBranchHead, e => branch.predicate(e.Item));//TODO AAAAA ctach

                tailBranches[branchId] = newBranch.Current.ExecutionBlock;

                newBranch.Current.ExecutionBlock.LinkTo(mergeBlock);
                newBranch.Current.ExecutionBlock.Completion.ContinueWith(x =>
                {
                    if(tailBranches.All(e=>e.Completion.IsCompleted))
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

            //mergeBlock.Completion.ContinueWith(x =>
            //{
            //    Console.Write("");
            //});

            return new StageSetupOut<TFirstInput, TNexTOutput>(new StageOut<TNexTOutput>(mergeBlock, Current.CancellationToken)
            {
                Previous = Current
            }, JobService);
        }

        #endregion

        private StageSetupOut<TFirstInput, TNexTOutput> CreateNextStageAsync<TNexTOutput>(IPipelineJobAsync<TOutput, TNexTOutput> asyncJob)
        {
            TransformBlock<PipelineItem<TOutput>, PipelineItem<TNexTOutput>> rePostBlock = null;
            void RePostMessage(PipelineItem<TOutput> message)
            {
                rePostBlock?.Post(message);
            }

            var nextBlock = new TransformBlock<PipelineItem<TOutput>, PipelineItem<TNexTOutput>>(
                async e => await asyncJob.InternalExecute(e, () => RePostMessage(e), Current.CancellationToken),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = asyncJob.MaxDegreeOfParallelism
                });

            rePostBlock = nextBlock;

            return CreateNextBlock(nextBlock);
        }

        private StageSetupOut<TFirstInput, TNexTOutput> CreateNextStage<TNexTOutput>(IPipelineJobSync<TOutput, TNexTOutput> job)
        {
            var buffer = new BatchBlockEx<PipelineItem<TOutput>>(7, 5000); //TODO

            TransformManyBlock<IEnumerable<PipelineItem<TOutput>>, PipelineItem<TNexTOutput>> rePostBlock = null;
            void RePostMessages(IEnumerable<PipelineItem<TOutput>> messages)
            {
                rePostBlock?.Post(messages);
            }

            var nextBlock = new TransformManyBlock<IEnumerable<PipelineItem<TOutput>>, PipelineItem<TNexTOutput>>(
                async e => await job.InternalExecute(e, () => RePostMessages(e), Current.CancellationToken),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = job.MaxDegreeOfParallelism
                });
            
            buffer.LinkTo(nextBlock, new DataflowLinkOptions() { PropagateCompletion = true });
            rePostBlock = nextBlock;

            buffer.Completion.ContinueWith(x =>
            {
                nextBlock.Complete();
            }, Current.CancellationToken);

            return CreateNextBlock(DataflowBlock.Encapsulate(buffer, nextBlock));
        }

        internal StageSetupOut<TFirstInput, TNexTOutput> CreateNextBlock<TNexTOutput>(IPropagatorBlock<PipelineItem<TOutput>, PipelineItem<TNexTOutput>> nextBlock)
        {
            Current.ExecutionBlock.LinkTo(nextBlock, new DataflowLinkOptions() { PropagateCompletion = true });

            Current.ExecutionBlock.Completion.ContinueWith(x =>
            {
                nextBlock.Complete();
            }, Current.CancellationToken);

            var nextStage = new StageOut<TNexTOutput>(nextBlock, Current.CancellationToken)
            {
                Previous = Current
            };

            Current.Next.Add(nextStage);

            return new StageSetupOut<TFirstInput, TNexTOutput>(nextStage, JobService);
        }

        public IAwaitablePipeline<TFirstInput, TOutput> From(CancellationToken cancellationToken)
        {
            var firstJobType = this.GetFirstStage().GetType();

            if (firstJobType.BaseType != null && firstJobType.GenericTypeArguments[0] == typeof(TFirstInput))
            {
                var firstStage = this.GetFirstStage();

                return new AwaitablePipeline<TFirstInput, TOutput>(((IStageIn<TFirstInput>)firstStage).ExecutionBlock, Current.ExecutionBlock, cancellationToken);
            }

            if (firstJobType.BaseType != null)
            {
                throw new Exception(
                    $"Stages config expects '{firstJobType.GenericTypeArguments[0].Name}', but was received '{typeof(TFirstInput).Name}'");
            }
            else
            {
                throw new Exception(
                    $"Stages config didn't expected '{typeof(TFirstInput).Name}' as input");
            }
        }

        public IAwaitablePipeline<TFirstInput, TOutput> From() => From(CancellationToken.None);
    }

    public class StageSetup<TFirstInput, TInput, TOutput> : StageSetupOut<TFirstInput, TOutput>, IStageSetupOut<TOutput>, IStageSetupIn<TInput>
    {

        private readonly IStage<TInput, TOutput> _stage;

        public StageSetup(IStage<TInput, TOutput> stage, IJobService jobService) : base(stage, jobService)
        {
            _stage = stage;
        }

        IStageOut<TOutput> IStageSetupOut<TOutput>.Current => _stage;

        IStageIn<TInput> IStageSetupIn<TInput>.Current => _stage;
    }

    //public class StageSetup<TFirstInput, TInput, TOutput> : StageSetupOut<TFirstInput, TOutput> , StageSetupIn<TFirstInput, TInput>
    //{
    //    private readonly IStage<TInput, TOutput> _stage;
    //    private readonly IJobService _jobService;

    //    private IJobService GetJobService
    //    {
    //        get
    //        {
    //            if (_jobService == null)
    //            {
    //                throw new Exception($"'{nameof(IJobService)}' isn't provided, if you need to use Generic stage setups, provide service.");
    //            }

    //            return _jobService;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets the current stage.
    //    /// </summary>
    //    public IStage<TInput, TOutput> Current => _stage;

    //    IStageIn<TInput> IStageSetupIn<TInput>.Current => Current;

    //    IStageOut<TOutput> IStageSetupOut<TOutput>.Current => Current;


    //    internal StageSetup(IStage<TInput, TOutput> stage, IJobService jobService)
    //    {
    //        _stage = stage;
    //        _jobService = jobService;
    //    }

    //    #region Generic Stages

    //    public StageSetupOut<TFirstInput,TNexTOutput> Stage<TJob, TNexTOutput>()
    //        where TJob : Job<TOutput, TNexTOutput>
    //        => CreateNextStage<TNexTOutput>(GetJobService.GetJobInstance<TJob>());

    //    public StageSetupOut<TFirstInput,TOutput> Stage<TJob>()
    //        where TJob : Job<TOutput, TOutput>
    //        => CreateNextStage<TOutput>(GetJobService.GetJobInstance<TJob>());

    //    //public StageSetupOut<TFirstInput,IEnumerable<TNexTOutput>> Stage<TJob, TJob2, TNexTOutput>()
    //    //    where TJob : Job<TOutput, TNexTOutput>
    //    //    where TJob2 : Job<TOutput, TNexTOutput>
    //    //    => Stage(GetJobService.GetJobInstance<TJob>(), GetJobService.GetJobInstance<TJob2>());

    //    //public StageSetupOut<TFirstInput,IEnumerable<TNexTOutput>> Stage<TJob, TJob2, TJob3, TNexTOutput>()
    //    //    where TJob : Job<TOutput, TNexTOutput>
    //    //    where TJob2 : Job<TOutput, TNexTOutput>
    //    //    where TJob3 : Job<TOutput, TNexTOutput>
    //    //    => Stage(GetJobService.GetJobInstance<TJob>(), GetJobService.GetJobInstance<TJob2>(), GetJobService.GetJobInstance<TJob3>());

    //    //public StageSetupOut<TFirstInput,IEnumerable<TNexTOutput>> Stage<TJob, TJob2, TJob3, TJob4, TNexTOutput>()
    //    //    where TJob : Job<TOutput, TNexTOutput>
    //    //    where TJob2 : Job<TOutput, TNexTOutput>
    //    //    where TJob3 : Job<TOutput, TNexTOutput>
    //    //    where TJob4 : Job<TOutput, TNexTOutput>
    //    //    => Stage(GetJobService.GetJobInstance<TJob>(), GetJobService.GetJobInstance<TJob2>(), GetJobService.GetJobInstance<TJob3>(), GetJobService.GetJobInstance<TJob4>());

    //    //public StageSetup<TOutput, TOutput> AsyncStage<TAsyncJob>(Func<TOutput, bool> condition = null)
    //    //    where TAsyncJob : AsyncJob<TOutput, TOutput>
    //    //    => CreateNextStageAsync<TOutput>(GetJobService.GetJobInstance<TAsyncJob>(), condition);

    //    public StageSetup<TOutput, TNexTOutput> AsyncStage<TAsyncJob, TNexTOutput>(Func<TOutput, bool> condition = null)
    //        where TAsyncJob : AsyncJob<TOutput, TNexTOutput>
    //        => CreateNextStageAsync<TNexTOutput>(GetJobService.GetJobInstance<TAsyncJob>(), condition);

    //    //public StageSetup<TOutput, TNexTOutput> AsyncStage<TAsyncJob, TAsyncJob2, TNexTOutput>()
    //    //    where TAsyncJob : AsyncJob<TOutput, TNexTOutput>
    //    //    where TAsyncJob2 : AsyncJob<TOutput, TNexTOutput>
    //    //    => AsyncStage(GetJobService.GetJobInstance<TAsyncJob>(), GetJobService.GetJobInstance<TAsyncJob2>());

    //    //public StageSetup<TOutput, TNexTOutput> AsyncStage<TAsyncJob, TAsyncJob2, TAsyncJob3, TNexTOutput>()
    //    //    where TAsyncJob : AsyncJob<TOutput, TNexTOutput>
    //    //    where TAsyncJob2 : AsyncJob<TOutput, TNexTOutput>
    //    //    where TAsyncJob3 : AsyncJob<TOutput, TNexTOutput>
    //    //    => AsyncStage(GetJobService.GetJobInstance<TAsyncJob>(), GetJobService.GetJobInstance<TAsyncJob2>(), GetJobService.GetJobInstance<TAsyncJob3>());

    //    //public StageSetup<TOutput, TNexTOutput> AsyncStage<TAsyncJob, TAsyncJob2, TAsyncJob3, TAsyncJob4, TNexTOutput>()
    //    //    where TAsyncJob : AsyncJob<TOutput, TNexTOutput>
    //    //    where TAsyncJob2 : AsyncJob<TOutput, TNexTOutput>
    //    //    where TAsyncJob3 : AsyncJob<TOutput, TNexTOutput>
    //    //    where TAsyncJob4 : AsyncJob<TOutput, TNexTOutput>
    //    //    => AsyncStage(GetJobService.GetJobInstance<TAsyncJob>(), GetJobService.GetJobInstance<TAsyncJob2>(), GetJobService.GetJobInstance<TAsyncJob3>(), GetJobService.GetJobInstance<TAsyncJob4>());

    //    //public StageSetup<TOutput, TNexTOutput> MapAs<TNexTOutput>()
    //    //    where TNexTOutput : class
    //    //    => AsyncStage(output => output as TNexTOutput);

    //    #endregion

    //    #region Nongeneric Stages

    //    public StageSetupOut<TFirstInput,TNexTOutput> Stage<TNexTOutput>(Job<TOutput, TNexTOutput> job)
    //        => CreateNextStage<TNexTOutput>(job);

    //    //public StageSetupOut<TFirstInput,IEnumerable<TNexTOutput>> Stage<TNexTOutput>(Func<IEnumerable<TOutput>, IEnumerable<TNexTOutput>> func)
    //    //    => Stage(new LambdaJob<TOutput, TNexTOutput>(func));

    //    //public StageSetupOut<TFirstInput,IEnumerable<TNexTOutput>> Stage<TNexTOutput>(params Job<TOutput, TNexTOutput>[] jobs)
    //    //    => Stage(new ConditionJob<TOutput, TNexTOutput>(jobs));

    //    public StageSetup<TOutput, TNexTOutput> AsyncStage<TNexTOutput>(AsyncJob<TOutput, TNexTOutput> asyncJob, Func<TOutput, bool> condition = null)
    //        => CreateNextStageAsync<TNexTOutput>(asyncJob, condition);

    //    //public StageSetup<TOutput, TNexTOutput> AsyncStage<TNexTOutput>(Func<TOutput, TNexTOutput> func)
    //    //    => AsyncStage(new AsyncLambdaJob<TOutput, TNexTOutput>(func));

    //    //public StageSetup<TOutput, TNexTOutput> AsyncStage<TNexTOutput>(params AsyncJob<TOutput, TNexTOutput>[] asyncJobs)
    //    //    => AsyncStage(new ConditionAsyncJob<TOutput, TNexTOutput>(asyncJobs));

    //    #endregion

    //    #region Nongeneric Split

    //    #region Nongeneric Branch

    //    //public StageSetup<TNexTOutput, TNexTOutput> Branch<TNexTOutput>(
    //    //    params (Func<TOutput, bool> condition, Func<StageSetup<TInput, TOutput>, IStageSetupOut<TNexTOutput>> branch)[] branches)
    //    //{
    //    //    var filterBlock = new FilterBlock<TOutput, TOutput>();




    //    //    var mergeBlock = new SourceJoinBlock<TNexTOutput>();



    //    //    foreach (var branch in branches)
    //    //    {
    //    //        //var nextBlock = new TransformBlock<TOutput, TOutput>(e => e);

    //    //        //var nextStageSetup2 =
    //    //        //    new StageSetup<TOutput, TOutput>(
    //    //        //        new Stage<TOutput, TOutput>(null)
    //    //        //        , _jobService);

    //    //        var newBranch = branch.branch(this);

    //    //        filterBlock.LinkTo(Current.Next.ExecutionBlock, (output, target) =>
    //    //        {
    //    //            if (branch.condition(output))
    //    //            {
    //    //                while (!target.TryAdd(output))
    //    //                {

    //    //                }
    //    //            }
    //    //        });


    //    //        newBranch.Current.ExecutionBlock.LinkTo(mergeBlock);
    //    //        mergeBlock.AddSource(newBranch.Current.ExecutionBlock);
    //    //    }

    //    //    var nextStageSetup =
    //    //        new StageSetup<TOutput, TOutput>(
    //    //            new Stage<TOutput, TOutput>(filterBlock)
    //    //            , _jobService);

    //    //    Current.ExecutionBlock.LinkTo(filterBlock);
    //    //    Current.Next = nextStageSetup.Current;


    //    //    return new StageSetup<TNexTOutput, TNexTOutput>(new Stage<TNexTOutput, TNexTOutput>(mergeBlock)
    //    //    {
    //    //        Previous = (IStageOut<TNexTOutput>) Current //HACK
    //    //    },
    //    //        _jobService);
    //    //}

    //    #endregion

    //    #endregion

    //    #region Pipeline creation


    //    private StageSetupOut<TFirstInput,TNexTOutput> CreateNextStage<TNexTOutput>(Job<TOutput, TNexTOutput> job)
    //    {
    //        var nextBuffer = new BatchBlock<TOutput>(int.MaxValue);

    //        Current.ExecutionBlock.Completion.ContinueWith(f =>
    //        {
    //            if (f.IsFaulted)
    //            {
    //                //((IDataflowBlock)_step2A).Fault(t.Exception);
    //                //((IDataflowBlock)_step2B).Fault(t.Exception);
    //            }
    //            else
    //            {
    //                //nextBuffer.Complete();
    //            }
    //        });

    //        Current.ExecutionBlock.LinkTo(nextBuffer);

    //        var newcurrent = CreateNextBlock(nextBuffer, null);

    //        newcurrent.Current.ExecutionBlock.Completion.ContinueWith(f =>
    //        {
    //            if (f.IsFaulted)
    //            {
    //                //((IDataflowBlock)_step2A).Fault(t.Exception);
    //                //((IDataflowBlock)_step2B).Fault(t.Exception);
    //            }
    //            else
    //            {
    //                //nextBuffer.Complete();
    //            }
    //        });

    //        //var nextBlock = new TransformBlock<TOutput, TNexTOutput>((e, target) =>
    //        //{
    //        //    foreach (var result in job.Execute(e.ToArray()))
    //        //    {
    //        //        while (!target.TryAdd(result))
    //        //        {

    //        //        }
    //        //    }

    //        //    target.CompleteAdding();
    //        //});

    //        var g = new TransformManyBlock<IEnumerable<TOutput>, TNexTOutput>(e => job.Execute(e.ToArray()), new ExecutionDataflowBlockOptions(){ MaxDegreeOfParallelism =5});

    //        //ar nextBlock = new TransformBlock<TOutput[], IEnumerable<TNexTOutput>>(e => job.Execute(e));

    //        var t = newcurrent.CreateNextBlock(g, null);


    //        return t;
    //    }

    //    private StageSetup<TOutput, TNexTOutput> CreateNextStageAsync<TNexTOutput>(AsyncJob<TOutput, TNexTOutput> asyncJob, Func<TOutput, bool> condition)
    //    {
    //        var nextBlock = new TransformBlock<TOutput, TNexTOutput>(e => asyncJob.Execute(e), new ExecutionDataflowBlockOptions(){ MaxDegreeOfParallelism = asyncJob.MaxDegreeOfParallelism});

    //        return CreateNextBlock(nextBlock, condition);
    //    }

    //    //TODO HACK
    //    internal StageSetup<TOutput, TNexTOutput> CreateNextBlock<TNexTOutput>(IPropagatorBlock<TOutput, TNexTOutput> nextBlock, Func<TOutput, bool> condition)
    //    {
    //        if (condition == null)
    //        {
    //            Current.ExecutionBlock.LinkTo(nextBlock);
    //        }
    //        else
    //        {
    //            //Current.ExecutionBlock.LinkTo(nextBlock, (input, target) =>
    //            //{
    //            //    if (condition(input))
    //            //    {
    //            //        while (!target.TryAdd(input))
    //            //        {

    //            //        }

    //            //    }
    //            //});
    //            throw new NotImplementedException("TODO 1");
    //        }


    //        Current.ExecutionBlock.Completion.ContinueWith(t =>
    //        {
    //            if (t.IsFaulted)
    //            {
    //                //((IDataflowBlock)_step2A).Fault(t.Exception);
    //                //((IDataflowBlock)_step2B).Fault(t.Exception);
    //            }
    //            else
    //            {
    //                //nextBlock.Complete();
    //            }
    //        });

    //        var nextStage = new Stage<TOutput, TNexTOutput>(nextBlock)
    //        {
    //            Previous = Current
    //        };

    //        Current.Next = nextStage;

    //        return new StageSetup<TOutput, TNexTOutput>(nextStage, _jobService);
    //    }




    //    //private StageSetup<TOutput, TNexTOutput> CreateNextBlock<TNexTOutput>(ITarget<List<TOutput>> executionBlock)
    //    //{
    //    //    switch (Current.ExecutionBlock)
    //    //    {
    //    //        case TransformBlock<TInput, TOutput> transformBlock:
    //    //            transformBlock.LinkTo(executionBlock);
    //    //            break;
    //    //        case BatchInputBlock<TOutput> batchInputBlock:
    //    //            batchInputBlock.LinkTo(executionBlock);
    //    //            break;

    //    //    }

    //    //    return AppendStage<TNexTOutput>(
    //    //        new Stage<TOutput>(executionBlock)
    //    //        {
    //    //            Previous = new List<IStage>
    //    //            {
    //    //                Current
    //    //            }
    //    //        });
    //    //}

    //    //public StageSetup<TOutput, TNextOutput> AppendStage<TNextOutput>(IStage<TOutput, TNextOutput> stage)
    //    //{
    //    //    if (Current.Next == null)
    //    //    {
    //    //        Current.Next = new List<IStage>()
    //    //        {
    //    //            stage
    //    //        };
    //    //    }
    //    //    else
    //    //    {
    //    //        Current.Next.Add(stage);
    //    //    }

    //    //    return new StageSetup<TOutput, TNextOutput>(stage, _jobService);
    //    //}

    //    //private StageSetup<TOutput, TNexTOutput> CreateFilterStage<TNexTOutput>(IPipelineJob job, object[] attributes)
    //    //{
    //    //    var attribute = attributes[0] as PipelineFilterAttribute;

    //    //    Current.Next = new[]{new Stage(new PipelineFilterJobAsync<TOutput>(attribute))
    //    //    {
    //    //        Previous = new []{Current},
    //    //        Next = new []{new Stage(job)
    //    //        {
    //    //            Previous = new[]{ Current }
    //    //        }}
    //    //    }};

    //    //    // Wrap the new stage with a setup
    //    //    return new StageSetup<TOutput, TNexTOutput>(Current.Next.First().Next.First(), _jobService);
    //    //}

    //    #endregion
    //}
}
