//using System.Collections.Generic;
//using System.Threading;
//using PipelineLauncher.Abstractions.Collections;

//namespace PipelineLauncher.Collections
//{
//    internal class Queue<T> : IQueue<T>
//    {
//        private readonly List<T> _collection = new List<T>();

//        public void Add(T element, CancellationToken cancellationToken)
//        {
//            _collection.Add(element);
//        }

//        public IEnumerable<T> GetElements(CancellationToken cancellationToken)
//        {
//            return _collection;
//        }

//        public int Count => _collection.Count;

//        public void CompleteAdding()
//        {
//        }
//    }
//}
