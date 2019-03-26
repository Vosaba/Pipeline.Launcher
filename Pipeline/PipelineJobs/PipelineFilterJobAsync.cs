using System;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Attributes;
using PipelineLauncher.Dto;

namespace PipelineLauncher.PipelineJobs
{
    internal class PipelineFilterJobAsync<TInput> : PipelineJob<TInput, TInput>, IPipelineFilterAsync
    {
        private readonly PipelineFilterAttribute _pipelineFilter;

        public PipelineFilterJobAsync(PipelineFilterAttribute pipelineFilter)
        {
            _pipelineFilter = pipelineFilter;
        }

        public object InternalExecute(object input, CancellationToken cancellationToken)
        {
            var result = _pipelineFilter.Execute(input);

            switch (result)
            {
                case RemoveResult _:
                    break;
                case KeepResult _:
                    Output.Add(input, cancellationToken);
                    break;
                case SkipResult _:
                    Output.Add(new StageSkipObject(input), cancellationToken);
                    break;
                case SkipToResult skipTo:
                    Output.Add(new StageSkipObject(input, skipTo.JobType), cancellationToken);
                    break;
            }
            
            return input;
        }

        public int MaxDegreeOfParallelism => Environment.ProcessorCount;
    }
}
