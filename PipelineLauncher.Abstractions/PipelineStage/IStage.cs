using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.Stages;
using System.Threading;
using System.Threading.Tasks;

namespace PipelineLauncher.Abstractions.PipelineStage
{
    public interface IStage<TInput, TOutput> : IPipelineStage<TInput, TOutput>
    {
        Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken);
        StageConfiguration Configuration { get; }
    }
}