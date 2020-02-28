namespace PipelineLauncher.Abstractions.Dto
{
    public interface IPipelinePredicate
    {

    }

    public interface IPipelinePredicate<T>: IPipelinePredicate
    {
        public delegate PredicateResult PipelinePredicate<in T>(T obj);
    }

    public delegate PredicateResult PipelinePredicate<in T>(T obj);

    public enum PredicateResult
    {
        Keep,
        Skip,
        Remove
    }
}