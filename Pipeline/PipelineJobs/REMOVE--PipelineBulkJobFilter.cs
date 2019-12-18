using System;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Configurations;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Dto;
using PipelineLauncher.Jobs;

namespace PipelineLauncher.PipelineJobs
{
    //internal class PipelineBulkJobFilter<TInput> : BulkJob<TInput, TInput>, IPipelineBulkFilter<TInput, TInput>
    //{
    //    private readonly PipelineFilterAttribute _pipelineFilter;

    //    public PipelineBulkJobFilter(PipelineFilterAttribute pipelineFilter)
    //    {
    //        _pipelineFilter = pipelineFilter;
    //    }

    //    public async Task<PipelineItem<TInput>> InternalExecute(PipelineItem<TInput> input, Action reExecute, CancellationToken cancellationToken)
    //    {
    //        //var result = _pipelineFilter.Execute(input);

    //        //switch (result)
    //        //{
    //        //    case RemoveResult _:
    //        //        break;
    //        //    case KeepResult _:
    //        //        Output.Add(input, cancellationToken);
    //        //        break;
    //        //    case SkipResult _:
    //        //        Output.Add(new StageSkipObject(input), cancellationToken);
    //        //        break;
    //        //    case SkipToResult skipTo:
    //        //        Output.Add(new StageSkipObject(input, skipTo.JobType), cancellationToken);
    //        //        break;
    //        //}
            
    //        return input;
    //    }

    //    public int MaxDegreeOfParallelism => Environment.ProcessorCount;

    //    JobConfiguration IPipelineBulkJob<TInput, TInput>.Configuration => throw new NotImplementedException();
    //}
}
