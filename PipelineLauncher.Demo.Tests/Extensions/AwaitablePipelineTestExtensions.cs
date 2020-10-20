using PipelineLauncher.Abstractions.PipelineRunner;
using PipelineLauncher.Demo.Tests.PipelineTest;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PipelineLauncher.Demo.Tests.Extensions
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

            pipelineRunner.SkippedItemReceivedEvent += PipelineRunner_SkippedItemReceivedEvent;
            // Process items
            var result = pipelineRunner.Process(items).ToArray();



            // Print elapsed time and result
            pipelineTest.StopTimerAndPrintResult(printInputItems ? (IEnumerable)items : result, stopWatch);
        }

        private static void PipelineRunner_SkippedItemReceivedEvent(Abstractions.PipelineEvents.SkippedItemEventArgs args)
        {
            throw new System.NotImplementedException();
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
