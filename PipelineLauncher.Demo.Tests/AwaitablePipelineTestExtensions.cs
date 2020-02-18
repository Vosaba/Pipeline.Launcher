using PipelineLauncher.Abstractions.PipelineRunner;
using System.Collections.Generic;
using System.Linq;
using PipelineLauncher.Demo.Tests.PipelineSetup;
using System.Collections;

namespace PipelineLauncher.Demo.Tests
{
    public static class AwaitablePipelineTestExtensions
    {
        public static void ProcessAndPrintResults<TInput, TOutput>(
            this IAwaitablePipelineRunner<TInput, TOutput> pipelineRunner,
            IEnumerable<TInput> items,
            PipelineTestBase pipelineTest,
            bool printInputItems = false)
        {
            // Start timer
            var stopWatch = pipelineTest.StartTimer();

            // Process items
            var result = pipelineRunner.Process(items).ToArray();

            // Print elapsed time and result
            pipelineTest.StopTimerAndPrintResult(printInputItems ? (IEnumerable)items : result, stopWatch);
        }

        public static void ProcessAndPrintResults<TInput, TOutput>(
            this (PipelineTestBase PipelineTest, IAwaitablePipelineRunner<TInput, TOutput> PipelineRunner) testAndRunner, 
            IEnumerable<TInput> items,
            bool printInputItems = false)
        {
            var (pipelineTest, pipelineRunner) = testAndRunner;

            ProcessAndPrintResults(pipelineRunner, items, pipelineTest, printInputItems);
        }
    }
}
