using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;

namespace PipelineLauncher.Abstractions.Stages
{
    public interface IStage
    {
    }

    public interface ISourceStage<TOutput> : IStage
    {

    }

    public interface ITargetStage<TInput> : IStage
    {

    }

    public interface IBulkStage<TInput, TOutput> : ITargetStage<TInput>, ISourceStage<TOutput>
    {
        Task<IEnumerable<TOutput>> ExecuteAsync(TInput[] input, CancellationToken cancellationToken);
        BulkStageConfiguration Configuration { get; }
    }
}