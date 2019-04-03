using System;
using System.Collections.Generic;
using PipelineLauncher.Dataflow.Enumeration;

namespace PipelineLauncher.Dataflow
{
    /// <summary>
    /// Take single items and create a list to pass on
    /// This will not parallelize the input as you could get more than requested
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    public class BatchInputBlock<TIn> : ExecutionBlock<TIn>, ITarget<TIn, List<TIn>>, IDisposable
    {
        private readonly int _batchSize;
        protected ITarget<List<TIn>> Target { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="batchSize"></param>
        public BatchInputBlock(int batchSize)
            : this(new QueueBlock<TIn>(), batchSize)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="batchSize"></param>
        public BatchInputBlock(IDataBlockCollection<TIn> source, int batchSize)
            : base(source)
        {
            _batchSize = batchSize;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        public void LinkTo(ITarget<List<TIn>> target)
        {
            Target = target;
        }

        public void LinkTo(ITarget<List<TIn>> target, Action<List<TIn>, ITarget<List<TIn>>> filterMethod)
        {
            LinkTo(target);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void Execute()
        {
            State = ExecutionState.Running;

            var itemList = new List<TIn>(_batchSize + 1);

            while (State == ExecutionState.Running)
            {
                try
                {
                    if (Source.Count == 0 && Source.IsCompleted)
                    {
                        State = ExecutionState.Done;
                        continue;
                    }

                    var t = Source.GetConsumingEnumerable();
                    foreach (var item in t)
                    {
                        itemList.Add(item);

                        if (itemList.Count < _batchSize)
                            continue;

                        Target.TryAdd(itemList);
                        itemList = new List<TIn>(_batchSize + 1);
                    }
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

            if (itemList.Count > 0)
                Target.TryAdd(itemList);

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