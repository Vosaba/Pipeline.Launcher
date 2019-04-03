using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PipelineLauncher.Dataflow.Enumeration;

namespace PipelineLauncher.Dataflow
{
    /// <summary>
    /// Take an item and output each item individually, transforming the item optionally
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    public class SourceJoinBlock<TIn> : ExecutionBlock<TIn>, ITarget<TIn, TIn>, IDisposable
    {
        protected ITarget<TIn> Target { get; set; }
        protected List<IComplete> Sources { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public SourceJoinBlock()
            : this(new QueueBlock<TIn>())
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        public SourceJoinBlock(IDataBlockCollection<TIn> source)
            : base(source)
        {
            Sources = new List<IComplete>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        public void LinkTo(ITarget<TIn> target)
        {
            Target = target;
        }

        public void LinkTo(ITarget<TIn> target, Action<TIn, ITarget<TIn>> filterMethod)
        {
            LinkTo(target);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        public void AddSource(IComplete target)
        {
            Sources.Add(target);
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

                        Target.TryAdd(item);
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

        #region IComplete
        public override void CompleteAdding()
        {
            if (Sources.All(s => s.IsCompleted))
                base.CompleteAdding();
        }
        
        #endregion

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
                Sources.Clear();
                base.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}