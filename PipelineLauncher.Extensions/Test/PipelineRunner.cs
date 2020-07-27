using System;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Abstractions.PipelineRunner;
using PipelineLauncher.Extensions.Infrastructure;

namespace PipelineLauncher.Extensions.Test
{
    public static class PipelineRunner
    {
        public const int MillisecondsTimeout = int.MaxValue;

        public static bool WaitForItemReceived<TInput, TOutput>(
            this IPipelineRunner<TInput, TOutput> pipelineRunner,
            Action postItemsToPipelineAction, 
            Predicate<TOutput> receiveExpectedItemPredicate = null,
            int millisecondsTimeout = MillisecondsTimeout)
        {
            var waitHandle = new AutoResetEvent(false);
            pipelineRunner.ItemReceivedEvent += args =>
            {
                if (receiveExpectedItemPredicate == null || receiveExpectedItemPredicate(args))
                {
                    waitHandle.Set();
                }
            };

            postItemsToPipelineAction();
            return waitHandle.WaitOne(millisecondsTimeout);
        }

        public static async Task<bool> WaitForItemReceivedAsync<TInput, TOutput>(
            this IPipelineRunner<TInput, TOutput> pipelineRunner,
            Action postItemsToPipelineAction,
            Predicate<TOutput> receiveExpectedItemPredicate = null,
            int millisecondsTimeout = MillisecondsTimeout)
        {
            var waitHandle = new AsyncAutoResetEvent(false);
            pipelineRunner.ItemReceivedEvent += args =>
            {
                if (receiveExpectedItemPredicate == null || receiveExpectedItemPredicate(args))
                {
                    waitHandle.Set();
                }
            };

            postItemsToPipelineAction();
            return await waitHandle.WaitAsync(TimeSpan.FromMilliseconds(millisecondsTimeout));
        }

        public static void WaitForItemsExceptionReceived<TInput, TOutput>(
            this IPipelineRunner<TInput, TOutput> pipelineRunner,
            Action postItemsToPipelineAction,
            Predicate<ExceptionItemsEventArgs<TOutput>> receiveExpectedExceptionPredicate = null,
            int millisecondsTimeout = MillisecondsTimeout)
        {
            var waitHandle = new AutoResetEvent(false);
            pipelineRunner.TypedHandler<TOutput>().ExceptionItemsReceivedEvent += args =>
            {
                if (receiveExpectedExceptionPredicate == null || receiveExpectedExceptionPredicate(args))
                {
                    waitHandle.Set();
                }
            };

            postItemsToPipelineAction();
            waitHandle.WaitOne(millisecondsTimeout);
        }

        public static async Task<bool> WaitForItemsExceptionReceivedAsync<TInput, TOutput>(
            this IPipelineRunner<TInput, TOutput> pipelineRunner,
            Action postItemsToPipelineAction,
            Predicate<ExceptionItemsEventArgs<TOutput>> receiveExpectedExceptionPredicate = null,
            int millisecondsTimeout = MillisecondsTimeout)
        {
            var waitHandle = new AsyncAutoResetEvent(false);
            pipelineRunner.TypedHandler<TOutput>().ExceptionItemsReceivedEvent += args =>
            {
                if (receiveExpectedExceptionPredicate == null || receiveExpectedExceptionPredicate(args))
                {
                    waitHandle.Set();
                }
            };

            postItemsToPipelineAction();
            return await waitHandle.WaitAsync(TimeSpan.FromMilliseconds(millisecondsTimeout));
        }
    }
}