using System;
using System.Threading.Tasks;
using PipelineLauncher.Dataflow.Enumeration;

namespace PipelineLauncher.Dataflow
{
    /// <summary>
    /// Perform an action that doesn't require the data to be passed on.
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    public class ActionBlock<TIn> : ExecutionBlock<TIn>, ITargetIn<TIn>
    {
        protected Action<TIn> Method { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        public ActionBlock(Action<TIn> method)
            : this(new QueueBlock<TIn>(), method)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="method"></param>
        public ActionBlock(IDataBlockCollection<TIn> source, Action<TIn> method)
            : base(source)
        {
            Method = method;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void Execute()
        {
            State = ExecutionState.Running;

            while (State == ExecutionState.Running)
            {
                try
                {
                    if (Source.Count == 0 && Source.IsCompleted)
                    {
                        State = ExecutionState.Done;
                        continue;
                    }

                    Parallel.ForEach(Source.GetConsumingEnumerable(), ParallelOptions, item =>
                    {
                        if (item != null)
                            Method(item);
                    });
                }
                catch (OperationCanceledException)
                {
                    State = ExecutionState.Cancel;
                }
                catch (Exception ex)
                {
                    State = ExecutionState.Error;
                    Exceptions.Add(ex.GetBaseException());
                    ParallelOptions.Cancel();
                }
            }
        }
    }
}