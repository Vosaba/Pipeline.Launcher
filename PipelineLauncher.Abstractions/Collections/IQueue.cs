using System.Collections.Generic;
using System.Threading;

namespace PipelineLauncher.Abstractions.Collections
{
    /// <summary>
    /// Represents a queue of elements
    /// </summary>
    /// <typeparam name="T">element type</typeparam>
    public interface IQueue<T>
    {

        /// <summary>
        /// Adds the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        void Add(T element, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the elements.
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> GetElements(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the element count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        int Count { get; }

        /// <summary>
        /// Completes the adding.
        /// </summary>
        void CompleteAdding();
    }
}
