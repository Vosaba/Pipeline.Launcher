using System;

namespace PipelineLauncher.Dataflow
{
    internal class Filter<TIn> : IDisposable
    {
        public Action<TIn, ITargetIn<TIn>> FilterMethod { get; private set; }
        public ITargetIn<TIn> Target { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="filterMethod"></param>
        public Filter(ITargetIn<TIn> target, Action<TIn, ITargetIn<TIn>> filterMethod)
        {
            Target = target;
            FilterMethod = filterMethod;
        }

        #region IDisposable

        private bool _disposed;

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
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
                Target = null;
                FilterMethod = null;
            }

            _disposed = true;
        }

        #endregion
    }

    public class FF
    {
        public FF()
        {
            
        }
    }
}
