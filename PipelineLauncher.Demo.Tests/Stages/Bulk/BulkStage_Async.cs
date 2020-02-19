using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Stages;

namespace PipelineLauncher.Demo.Tests.Stages.Bulk
{
    public class BulkStage_Async : BulkStage<Item>
    {
        // TODO: Use new BulkStage with IAsyncEnumerable
        public override async Task<IEnumerable<Item>> ExecuteAsync(Item[] items)
        {
            await Task.Delay(1000);

            foreach (var item in items)
            {
                item.Process(GetType());
            }

            return items;
        }
    }
}
