using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipelineLauncher.Pipelines
{
    public interface IPipeline<in TInput, TOutput>
    {
        List<Exception> Exceptions { get; }

        TOutput Run(TInput input);
        IEnumerable<TOutput> Run(IEnumerable<TInput> input);

        /// <summary>
        /// Runs the pipeline with the single param.
        /// </summary>
        /// <param name="input">The param.</param>
        Task<TOutput> RunAsync(TInput input);
        /// <summary>
        /// Runs the pipeline with the specified params.
        /// </summary>
        /// <param name="input">The param.</param>
        Task<IEnumerable<TOutput>> RunAsync(IEnumerable<TInput> input);
    }
}
