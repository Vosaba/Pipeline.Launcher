using PipelineLauncher.Dto;
using PipelineLauncher.PipelineSetup;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PipelineLauncher.Extensions
{
    public static class PipelineSetupExtensions
    {
        public static IPipelineSetup<TInput, TOutput> RemoveDuplicates<TInput, TOutput>(this IPipelineSetup<TInput, TOutput> pipelineSetup, Func<TOutput, int> hashFunc = null)
        {
            var processedHash = new ConcurrentDictionary<int, byte>();

            return pipelineSetup.AsyncStage((TOutput input, AsyncJobOption<TOutput, TOutput> asyncJobOption) =>
            {
                var hash = hashFunc?.Invoke(input) ?? input.GetHashCode();

                return !processedHash.TryAdd(hash, byte.MinValue) ? asyncJobOption.Remove(input) : input;
            });
        }

        public static IPipelineSetup<TInput, TOutput> RemoveDuplicatesPermanent<TInput, TOutput>(this IPipelineSetup<TInput, TOutput> pipelineSetup, Func<TOutput, int> hashFunc = null)
        {
            var processedHash = new ConcurrentDictionary<int, byte>();

            return pipelineSetup.Stage((IEnumerable<TOutput> inputs) =>
            {
                var result = new List<TOutput>();

                foreach(var input in inputs)
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
            return pipelineSetup.AsyncStage(output => output as TCast);
        }
    }
}