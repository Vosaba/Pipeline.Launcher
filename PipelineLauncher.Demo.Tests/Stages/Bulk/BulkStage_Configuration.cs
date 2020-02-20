using System.Collections.Generic;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Stages;

namespace PipelineLauncher.Demo.Tests.Stages.Bulk
{
    public class BulkStage_Configuration : BulkStage<Item>
    {
        public override IEnumerable<Item> Execute(Item[] items)
        {
            foreach (var item in items)
            {
                item.Process(GetType());
            }

            return items;
        }

        public override BulkStageConfiguration Configuration 
            => new BulkStageConfiguration 
            {
                BatchSize = 5,
                BatchTimeOut = 1000
            };
    }
}
