//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Threading;
//using PipelineLauncher.Abstractions.Collections;

//namespace PipelineLauncher.Collections
//{
//    internal class BlockingQueue<T> : IQueue<T>
//    {
//        private readonly BlockingCollection<T> _collection = new BlockingCollection<T>();

//        public void Add(T element, CancellationToken cancellationToken)
//        {
//            _collection.Add(element, cancellationToken);
//        }

//        public IEnumerable<T> GetElements(CancellationToken cancellationToken)
//        {
//            return _collection.GetConsumingEnumerable(cancellationToken);
//        }

//        public void CompleteAdding()
//        {
//            _collection.CompleteAdding();
//        }

//        public int Count => _collection.Count;
//    }
//}
