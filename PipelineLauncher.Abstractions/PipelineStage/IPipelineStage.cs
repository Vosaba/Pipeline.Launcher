using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.Abstractions.PipelineStage
{
    
    public interface IConditionalStage<TInput> 
    {
        PredicateResult Predicate(TInput input);
    }
}
