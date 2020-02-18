namespace PipelineLauncher.Abstractions.Dto
{
    public delegate PredicateResult PipelinePredicate<in T>(T obj);

    public enum PredicateResult
    {
        Keep,
        Skip,
        Remove
    }
}