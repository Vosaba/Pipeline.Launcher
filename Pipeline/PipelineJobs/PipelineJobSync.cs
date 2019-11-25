using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Attributes;
using PipelineLauncher.Exceptions;

namespace PipelineLauncher.PipelineJobs
{
    public abstract class PipelineJobSync<TInput, TOutput> : PipelineJob<TInput, TOutput>, IPipelineJobSync<TInput, TOutput>
    {
        public abstract Task<IEnumerable<TOutput>> ExecuteAsync(IEnumerable<TInput> input, CancellationToken cancellationToken);

        public virtual async Task<IEnumerable<PipelineItem<TOutput>>> InternalExecute(IEnumerable<PipelineItem<TInput>> input, CancellationToken cancellationToken)
        {
            var result = new List<PipelineItem<TOutput>>();
            var inputArray = input.ToArray();

            try
            {
                var removeAndExceptionItems = inputArray.Where(e =>
                {
                    var type = e.GetType();
                    return type == typeof(RemoveItem<TInput>) || type == typeof(ExceptionItem<TInput>);
                }).Cast<NonResultItem<TInput>>().Select(e => e.Return<TOutput>());
                result.AddRange(removeAndExceptionItems);

                //var exceptionItems = inputArray.Where(e => e.GetType() == typeof(ExceptionItem<TInput>)).Cast<ExceptionItem<TInput>>().Select(e => e.Return<TOutput>());
                //result.AddRange(exceptionItems);

                var output = await ExecuteAsync(inputArray.Where(e => e.GetType() == typeof(PipelineItem<TInput>)).Select(e => e.Item), cancellationToken);
                result.AddRange(output.Select(e => new PipelineItem<TOutput>(e)));


                return result;
            }
            catch (Exception ex)
            {
                return new[] { new ExceptionItem<TOutput>(ex, inputArray.Select(e => e.Item)) };
            }
        }
    }
}