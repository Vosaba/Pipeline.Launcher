namespace PipelineLauncher.Exceptions
{
    public class StageReTryCountException: PipelineRunTimeException
    {
        public int RetriesCount { get; }

        public StageReTryCountException(int retriesCount) : base(string.Format(Helpers.Strings.RetryOnAwaitable, retriesCount))
        {
            RetriesCount = retriesCount;
        }
    }
}