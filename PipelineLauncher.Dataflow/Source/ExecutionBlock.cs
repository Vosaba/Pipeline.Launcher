using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataflowLite;
using PipelineLauncher.Dataflow.Enumeration;

namespace PipelineLauncher.Dataflow
{
    /// <summary>
    /// Abstract class on which to build a block that will perform some kind of task
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    public abstract class ExecutionBlock<TIn> : IComplete, IExecutionBlock
    {
        public List<Exception> Exceptions { get; protected set; } = new List<Exception>();

        public string Title { get; set; }

        public ExecutionState State { get; protected set; }

        protected IDataBlockCollection<TIn> Source { get; private set; }

        public BlockParallelOption ParallelOptions { get; }

        public Task ExecutionTask { get; private set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        protected ExecutionBlock(IDataBlockCollection<TIn> source)
        {
            Source = source;
            ParallelOptions = new BlockParallelOption();
            ExecutionTask = Task.Run(() => Execute(), ParallelOptions.CancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        protected abstract void Execute();

        /// <summary>
        /// 
        /// </summary>
        public void CancelExecution()
        {
            ParallelOptions.Cancel();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Wait()
        {
            ExecutionTask.Wait();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected bool IsTaskRunning()
        {
            if (ExecutionTask == null)
                return false;

            var state = ExecutionTask.Status;
            switch (state)
            {
                case TaskStatus.Created:
                case TaskStatus.WaitingForActivation:
                case TaskStatus.WaitingToRun:
                case TaskStatus.Running:
                    return true;

                case TaskStatus.WaitingForChildrenToComplete:
                case TaskStatus.RanToCompletion:
                case TaskStatus.Canceled:
                case TaskStatus.Faulted:
                    return false;

                default:
                    return false;
            }
        }

        #region ITryAdd
        public bool TryAdd(TIn item)
        {
            return !Source.IsCompleted && Source.TryAdd(item);
        }

        public bool TryAdd(TIn item, int timeout)
        {
            return !Source.IsCompleted && Source.TryAdd(item, timeout);
        }

        public bool TryAdd(TIn item, int timeout, CancellationToken cancellationToken)
        {
            return !Source.IsCompleted && Source.TryAdd(item, timeout, cancellationToken);
        }
        #endregion

        #region IComplete
        public virtual void CompleteAdding()
        {
            Source.CompleteAdding();
        }

        public virtual bool IsCompleted
        {
            get { return Source.IsCompleted; }
        }
        #endregion

        #region IDisposable

        private bool _disposed;

        /// <summary>
        /// 
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Source?.Dispose();
                Source = null;
            }

            _disposed = true;
        }

        #endregion
    }
}
