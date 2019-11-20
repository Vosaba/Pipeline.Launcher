using System;
using System.Threading;
using System.Threading.Tasks;

namespace DataflowLite
{
    public class BlockParallelOption : ParallelOptions
    {
        public CancellationTokenSource CancellationSource { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public BlockParallelOption()
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxDegreeOfParallelism"></param>
        /// <returns></returns>
        public BlockParallelOption SetMaxDegreeOfParallelism(int maxDegreeOfParallelism)
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationSource"></param>
        /// <returns></returns>
        public BlockParallelOption SetCancellationSource(CancellationTokenSource cancellationSource)
        {
            CancellationSource = cancellationSource;
            CancellationToken = cancellationSource?.Token ?? default(CancellationToken);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskScheduler"></param>
        /// <returns></returns>
        public BlockParallelOption SetTaskScheduler(TaskScheduler taskScheduler)
        {
            TaskScheduler = taskScheduler;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Cancel()
        {
            CancellationSource?.Cancel();
        }
    }
}
