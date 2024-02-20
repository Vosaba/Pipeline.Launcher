using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;

namespace PipelineLauncher.Abstractions.Stages
{
    public interface IStage<TInput, TOutput> : ITargetStage<TInput>, ISourceStage<TOutput>
    {
        Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken);
        StageConfiguration Configuration { get; }
    }
}