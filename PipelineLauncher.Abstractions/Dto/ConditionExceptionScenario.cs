namespace PipelineLauncher.Abstractions.Dto
{
    public enum ConditionExceptionScenario
    {
        GoToNextCondition,
        StopPipelineExecution,
        AddExceptionAndGoToNextCondition
    }
}