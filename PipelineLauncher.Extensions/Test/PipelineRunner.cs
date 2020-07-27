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

            void OnPipelineRunnerOnItemReceivedEvent(TOutput args)
            {
                if (receiveExpectedItemPredicate == null || receiveExpectedItemPredicate(args))
                {
                    waitHandle.Set();
                }
            }

            pipelineRunner.ItemReceivedEvent += OnPipelineRunnerOnItemReceivedEvent;

            postItemsToPipelineAction();
            var result = waitHandle.WaitOne(millisecondsTimeout);
            pipelineRunner.ItemReceivedEvent -= OnPipelineRunnerOnItemReceivedEvent;

            return result;
        }

        public static async Task<bool> WaitForItemReceivedAsync<TInput, TOutput>(
            this IPipelineRunner<TInput, TOutput> pipelineRunner,
            Action postItemsToPipelineAction,
            Predicate<TOutput> receiveExpectedItemPredicate = null,
            int millisecondsTimeout = MillisecondsTimeout)
        {
            var waitHandle = new AsyncAutoResetEvent(false);

            void OnPipelineRunnerOnItemReceivedEvent(TOutput args)
            {
                if (receiveExpectedItemPredicate == null || receiveExpectedItemPredicate(args))
                {
                    waitHandle.Set();
                }
            }

            pipelineRunner.ItemReceivedEvent += OnPipelineRunnerOnItemReceivedEvent;

            postItemsToPipelineAction();
            var result = await waitHandle.WaitAsync(TimeSpan.FromMilliseconds(millisecondsTimeout));
            pipelineRunner.ItemReceivedEvent -= OnPipelineRunnerOnItemReceivedEvent;

            return result;
        }

        public static bool WaitForItemsExceptionReceived<TInput, TOutput>(
            this IPipelineRunner<TInput, TOutput> pipelineRunner,
            Action postItemsToPipelineAction,
            Predicate<ExceptionItemsEventArgs<TOutput>> receiveExpectedExceptionPredicate = null,
            int millisecondsTimeout = MillisecondsTimeout)
        {
            var waitHandle = new AutoResetEvent(false);

            void OnExceptionItemsReceivedEvent(ExceptionItemsEventArgs<TOutput> args)
            {
                if (receiveExpectedExceptionPredicate == null || receiveExpectedExceptionPredicate(args))
                {
                    waitHandle.Set();
                }
            }

            pipelineRunner.TypedHandler<TOutput>().ExceptionItemsReceivedEvent += OnExceptionItemsReceivedEvent;

            postItemsToPipelineAction();
            var result = waitHandle.WaitOne(millisecondsTimeout);
            pipelineRunner.TypedHandler<TOutput>().ExceptionItemsReceivedEvent -= OnExceptionItemsReceivedEvent;

            return result;
        }

        public static async Task<bool> WaitForItemsExceptionReceivedAsync<TInput, TOutput>(
            this IPipelineRunner<TInput, TOutput> pipelineRunner,
            Action postItemsToPipelineAction,
            Predicate<ExceptionItemsEventArgs<TOutput>> receiveExpectedExceptionPredicate = null,
            int millisecondsTimeout = MillisecondsTimeout)
        {
            var waitHandle = new AsyncAutoResetEvent(false);

            void OnExceptionItemsReceivedEvent(ExceptionItemsEventArgs<TOutput> args)
            {
                if (receiveExpectedExceptionPredicate == null || receiveExpectedExceptionPredicate(args))
                {
                    waitHandle.Set();
                }
            }

            pipelineRunner.TypedHandler<TOutput>().ExceptionItemsReceivedEvent += OnExceptionItemsReceivedEvent;

            postItemsToPipelineAction();
            var result = await waitHandle.WaitAsync(TimeSpan.FromMilliseconds(millisecondsTimeout));
            pipelineRunner.TypedHandler<TOutput>().ExceptionItemsReceivedEvent -= OnExceptionItemsReceivedEvent;

            return result;
        }
    }
}