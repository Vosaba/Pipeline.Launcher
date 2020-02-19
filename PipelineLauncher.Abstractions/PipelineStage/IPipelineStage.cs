using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.Stages;
using System.Threading;
using System.Threading.Tasks;

namespace PipelineLauncher.Abstractions.PipelineStage
{
    public interface IPipelineStage<TInput, TOutput> : IStage<TInput, TOutput>
    {
        Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken);
        StageConfiguration Configuration { get; }
    }
}