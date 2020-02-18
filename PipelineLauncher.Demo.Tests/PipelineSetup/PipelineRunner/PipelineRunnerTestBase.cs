using PipelineLauncher.Abstractions.PipelineRunner;
using PipelineLauncher.Demo.Tests.Items;
using System.Collections.Generic;
using Xunit.Abstractions;

namespace PipelineLauncher.Demo.Tests.PipelineSetup.PipelineRunner
{
    public class PipelineRunnerTestBase: PipelineTestBase
    {
        protected int DefaultLastItemIndex = 5;
        protected int DefaultItemsProcessedCount = 6;

        public PipelineRunnerTestBase(ITestOutputHelper output) : base(output) { }

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
