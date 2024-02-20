using PipelineLauncher.Extensions;

namespace PipelineLauncher.Exceptions
{
    public class StageRetryCountException: PipelineRunTimeException
    {
        public int RetriesCount { get; }

        public StageRetryCountException(int retriesCount) : base(string.Format(Helpers.Strings.RetriesMaxCountReached, retriesCount))
        {
            RetriesCount = retriesCount;
        }
    }
}