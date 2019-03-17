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

        public object InternalPerform(object param, CancellationToken cancellationToken)
        {
            var result = _pipelineFilter.Perform(param);

            switch (result)
            {
                case KeepResult _:
                    Output.Add(param, cancellationToken);
                    break;
                case SkipResult _:
                    Output.Add(new StageSkipObject(param), cancellationToken);
                    break;
                case SkipToResult skipTo:
                    Output.Add(new StageSkipObject(param, skipTo.JobType), cancellationToken);
                    break;
            }
            
            return param;
        }

        public int MaxDegreeOfParallelism => Environment.ProcessorCount;
    }
}
