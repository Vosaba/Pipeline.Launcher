namespace PipelineLauncher.Abstractions.Dto
{
    public enum ConditionExceptionScenario
    {
        GoToNextCondition,
        BreakPipelineExecution,
        AddExceptionAndGoToNextCondition
    }
}