using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using System.Threading;
using System.Threading.Tasks;

namespace PipelineLauncher.Abstractions.PipelineStage
{
    public interface IStage<TInput, TOutput> : ITargetStage<TInput>, ISourceStage<TOutput>
    {
        Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken);
        StageConfiguration Configuration { get; }
    }
}