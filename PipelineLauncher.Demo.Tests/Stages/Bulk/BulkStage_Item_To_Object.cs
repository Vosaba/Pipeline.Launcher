using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Stages;
using System.Collections.Generic;
using System.Linq;

namespace PipelineLauncher.Demo.Tests.Stages.Bulk
{
    public class BulkStage_Item_To_Object : BulkStage<Item, object>
    {
        public override IEnumerable<object> Execute(Item[] items)
        {
            foreach (var item in items)
            {
                item.Process(GetType());
            }

            return items.Cast<object>();
        }
    }
}
