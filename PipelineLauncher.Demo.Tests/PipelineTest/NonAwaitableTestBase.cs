using System.Collections.Generic;
using PipelineLauncher.Abstractions.PipelineRunner;
using PipelineLauncher.Demo.Tests.Extensions;
using PipelineLauncher.Demo.Tests.Items;
using Xunit.Abstractions;

namespace PipelineLauncher.Demo.Tests.PipelineTest
{
    public class NonAwaitableTestBase: PipelineTestBase
    {
        protected int DefaultLastItemIndex = 5;
        protected int DefaultItemsProcessedCount = 6;

        public NonAwaitableTestBase(ITestOutputHelper output) : base(output) { }

        protected bool StopExecutionConditionByTotalProcessed(ref int totalProcessedCount)
        {
            return ++totalProcessedCount == DefaultItemsProcessedCount;
        }

        protected bool StopExecutionConditionByLastIndex<TOutput>(TOutput item) where TOutput : Item
        {
            return item.Index == DefaultLastItemIndex;
        }

        protected void PostItemsAndPrintProcessedWithDefaultConditionToStop<TInput, TOutput>(
            IPipelineRunner<TInput, TOutput> pipelineRunner,
            IEnumerable<TInput> items)
        {
            var processedCount = 0;

            // Post items and retrieve WaitHandle
            var waitHandle = (this, pipelineRunner)
                .PostItemsAndPrintProcessed(items, x => StopExecutionConditionByTotalProcessed(ref processedCount));

            waitHandle.WaitOne();
        }
    }
}
