using System;
using System.Threading.Tasks;
using PipelineLauncher.Dataflow.Enumeration;

namespace PipelineLauncher.Dataflow
{
    /// <summary>
    /// Take a list of items and output each item individually, transforming the item optionally
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public class TransformToManyBlock<TIn, TOut> : ExecutionBlock<TIn>, ITarget<TIn, TOut>, IDisposable
    {
        protected Action<TIn, ITarget<TOut>> Method { get; set; }
        protected ITarget<TOut> Target { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        public TransformToManyBlock(Action<TIn, ITarget<TOut>> method)
            : this(new QueueBlock<TIn>(), method)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="method"></param>
        public TransformToManyBlock(IDataBlockCollection<TIn> source, Action<TIn, ITarget<TOut>> method)
            : base(source)
        {
            Method = method;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        public void LinkTo(ITarget<TOut> target)
        {
            Target = target;
        }

        public void LinkTo(ITarget<TOut> target, Action<TOut, ITarget<TOut>> filterMethod)
        {
            LinkTo(target);
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
                        if (item == null)
                            return;

                        Method(item, Target);
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

            Target.CompleteAdding();
        }


        #region IDisposable

        private bool _disposed;

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Target = null;
                base.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}