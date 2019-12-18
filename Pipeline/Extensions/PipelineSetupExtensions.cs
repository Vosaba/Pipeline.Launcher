﻿using PipelineLauncher.Dto;
using PipelineLauncher.PipelineSetup;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipelineLauncher.Extensions
{
    public static class PipelineSetupExtensions
    {
        public static IPipelineSetup<TInput, TOutput> RemoveDuplicates<TInput, TOutput>(this IPipelineSetup<TInput, TOutput> pipelineSetup, Func<TOutput, int> hashFunc = null)
        {
            var processedHash = new ConcurrentDictionary<int, byte>();

            return pipelineSetup.Stage((TOutput input, StageOption<TOutput, TOutput> asyncJobOption) =>
            {
                var hash = hashFunc?.Invoke(input) ?? input.GetHashCode();

                return !processedHash.TryAdd(hash, byte.MinValue) ? asyncJobOption.Remove(input) : input;
            });
        }

        public static IPipelineSetup<TInput, TOutput> RemoveDuplicatesPermanent<TInput, TOutput>(this IPipelineSetup<TInput, TOutput> pipelineSetup, Func<TOutput, int> hashFunc = null)
        {
            var processedHash = new ConcurrentDictionary<int, byte>();

            return pipelineSetup.BulkStage(inputs =>
            {
                var result = new List<TOutput>();

                foreach (var input in inputs)
                {
                    var hash = hashFunc?.Invoke(input) ?? input.GetHashCode();

                    if (processedHash.TryAdd(hash, byte.MinValue))
                    {
                        result.Add(input);
                    }
                }

                return result;
            });
        }

        public static IPipelineSetup<TInput, TCast> Cast<TInput, TOutput, TCast>(this IPipelineSetup<TInput, TOutput> pipelineSetup)
            where TCast : class
        {
            return pipelineSetup.Stage(output => output as TCast);
        }

        public static IPipelineSetup<TInput, TOutput> Delay<TInput, TOutput>(this IPipelineSetup<TInput, TOutput> pipelineSetup, long millisecondsDelay)
        {
            object @lock = new object();
            DateTime performedTime = DateTime.Now;

            return pipelineSetup.Stage(input =>
            {
                lock (@lock)
                {
                    var lastPerformedTime = performedTime;
                    while (lastPerformedTime.AddMilliseconds(millisecondsDelay) > DateTime.Now)
                    { }

                    performedTime = DateTime.Now;
                    return input;
                }
            });
        }

        public static IPipelineSetup<TInput, TOutput> BulkDelay<TInput, TOutput>(this IPipelineSetup<TInput, TOutput> pipelineSetup, long millisecondsDelay)
        {
            object @lock = new object();
            DateTime performedTime = DateTime.Now;

            return pipelineSetup.BulkStage(inputs =>
            {
                lock (@lock)
                {
                    var lastPerformedTime = performedTime;
                    while (lastPerformedTime.AddMilliseconds(millisecondsDelay) > DateTime.Now)
                    { }

                    performedTime = DateTime.Now;
                    return inputs;
                }
            });
        }

        public static IPipelineSetup<TInput, TOutput> CallWithTimeDelay<TInput, TOutput>(this IPipelineSetup<TInput, TOutput> pipelineSetup, int millisecondsDelay)
        {
            var processedHash = new ConcurrentDictionary<int, byte>();
            IPipelineSetup<TInput, TOutput> pipeLineConfig;

           var config  = pipelineSetup.Current.Previous.PipelineBaseConfiguration;



           pipeLineConfig = pipelineSetup.Stage(async (TOutput input) =>
            {
                await Task.Delay(millisecondsDelay);
                return input;
            });

            return pipeLineConfig;
        }
    }
}