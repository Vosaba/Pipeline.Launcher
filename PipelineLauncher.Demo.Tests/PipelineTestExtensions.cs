using PipelineLauncher.Abstractions.PipelineRunner;
using System.Collections.Generic;
using System.Linq;
using PipelineLauncher.Demo.Tests.PipelineSetup;
using System.Collections;
using System.Threading;
using System;
using System.Diagnostics;

namespace PipelineLauncher.Demo.Tests
{
    public static class PipelineTestExtensions
    {
        public static WaitHandle PostItemsAndPrintProcessed<TInput, TOutput>(
            this IPipelineRunner<TInput, TOutput> pipelineRunner,
            IEnumerable<TInput> items,
            PipelineTestBase pipelineTest,
            Func<TOutput, bool> stopExecutionCondition)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            // Start timer
            var stopWatch = pipelineTest.StartTimer();

            // Setup cancellationToken for finalize Pipeline execution
            pipelineRunner.SetupCancellationToken(cancellationTokenSource.Token);

            // Subscribe for processed items
            pipelineRunner.ItemReceivedEvent +=
                x => PipelineRunner_ItemReceivedEvent(x, pipelineTest, cancellationTokenSource, stopExecutionCondition, stopWatch);

            // Post items
            pipelineRunner.Post(items);

            // Return WaitHandle
            return cancellationTokenSource.Token.WaitHandle;
        }

        public static WaitHandle PostItemsAndPrintProcessed<TInput, TOutput>(
            this (PipelineTestBase PipelineTest, IPipelineRunner<TInput, TOutput> PipelineRunner) testAndRunner,
            IEnumerable<TInput> items,
            Func<TOutput, bool> stopExecutionCondition)
        {
            var (pipelineTest, pipelineRunner) = testAndRunner;

            return PostItemsAndPrintProcessed(pipelineRunner, items, pipelineTest, stopExecutionCondition);
        }

        private static void PipelineRunner_ItemReceivedEvent<TOutput>(
            TOutput item,
            PipelineTestBase pipelineTest,
            CancellationTokenSource cancellationTokenSource,
            Func<TOutput, bool> stopExecutionCondition,
            Stopwatch stopWatch)
        {
            pipelineTest.PrintProcessed(item);

            if (stopExecutionCondition(item))
            {
                cancellationTokenSource.Cancel();

                pipelineTest.StopTimerAndPrintElapsedTime(stopWatch);
            }
        }
    }
}
