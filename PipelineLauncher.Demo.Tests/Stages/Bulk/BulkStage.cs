using System.Collections.Generic;
using System.Threading;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Stages;

namespace PipelineLauncher.Demo.Tests.Stages.Bulk
{
    public class BulkStage : BulkStage<Item>
    {
        public override IEnumerable<Item> Execute(Item[] items)
        {
            foreach (var item in items)
            {
                item.Process(GetType());

                yield return item;
            }
        }

        public override BulkStageConfiguration Configuration => new BulkStageConfiguration
        {
            BatchItemsTimeOut = 10,
            BatchItemsCount = 100
        };
    }

    public class BulkStage_1 : BulkStage<Item>
    {
        public override IEnumerable<Item> Execute(Item[] items)
        {
            foreach (var item in items)
            {
                item.Process(GetType());

                yield return item;
            }
        }
    }

    public class BulkStage_2 : BulkStage<Item>
    {
        public override IEnumerable<Item> Execute(Item[] items)
        {
            foreach (var item in items)
            {
                item.Process(GetType());

                yield return item;
            }
        }
    }

    public class BulkStage_3 : BulkStage<Item>
    {
        public override IEnumerable<Item> Execute(Item[] items)
        {
            foreach (var item in items)
            {
                item.Process(GetType());

                yield return item;
            }
        }
    }
}
