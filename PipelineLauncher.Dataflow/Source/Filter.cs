using System;

namespace PipelineLauncher.Dataflow
{
    internal class Filter<TIn> : IDisposable
    {
        public Action<TIn, ITarget<TIn>> FilterMethod { get; private set; }
        public ITarget<TIn> Target { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="filterMethod"></param>
        public Filter(ITarget<TIn> target, Action<TIn, ITarget<TIn>> filterMethod)
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
}
