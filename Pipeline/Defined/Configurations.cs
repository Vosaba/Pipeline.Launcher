using PipelineLauncher.Abstractions.PipelineStage.Configurations;

namespace PipelineLauncher
{
    public static class Configurations
    {
        public static StageBaseConfiguration Ordered = new StageBaseConfiguration
        {
            EnsureOrdered = true
        };

        public static class BulkStage
        {
            public static BulkStageConfiguration TakeWhileInputAvailable = new BulkStageConfiguration
            {
                BatchSize = int.MaxValue,
                BatchTimeOut = int.MaxValue
            };

            public static BulkStageConfiguration TakeAllOrTimeout(int batchTimeOut) => new BulkStageConfiguration
            {
                BatchSize = int.MaxValue,
                BatchTimeOut = batchTimeOut
            };
        }
    }
}
