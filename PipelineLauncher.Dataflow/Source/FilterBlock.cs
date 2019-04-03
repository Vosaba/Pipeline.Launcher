using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineLauncher.Dataflow.Enumeration;

namespace PipelineLauncher.Dataflow
{
    /// <summary>
    /// Take an item and output each item individually, transforming the item optionally
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public class FilterBlock<TIn, TOut> : ExecutionBlock<TIn>, ITarget<TIn, TIn>, IDisposable
    {
        private List<Filter<TIn>> Filters { get; }

        /// <summary>
        /// 
        /// </summary>
        public FilterBlock()
            : this(new QueueBlock<TIn>())
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        public FilterBlock(IDataBlockCollection<TIn> source)
            : base(source)
        {
            Filters = new List<Filter<TIn>>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        public void LinkTo(ITarget<TIn> target)
        {
#if DEBUG
            throw new InvalidOperationException("LinkTo(target) is not valid for FilterBlock. You need to use LinkTo(target, method).");
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="filterMethod"></param>
        public void LinkTo(ITarget<TIn> target,  Action<TIn, ITarget<TIn>> filterMethod)
        {
            Filters.Add(new Filter<TIn>(target, filterMethod));
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

                        Filters.ForEach(f => f.FilterMethod(item, f.Target));
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

            Filters.ForEach(f =>
            {
                if (!f.Target.IsCompleted)
                    f.Target.CompleteAdding();
            });
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
                Filters.ForEach(f => f.Dispose());
                base.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}