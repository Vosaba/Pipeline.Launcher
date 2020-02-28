using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Stages;

namespace PipelineLauncher.Demo.Tests.Stages.Single
{
    public class StageSItemToItem2: StageS<Item, Item2>
    {
        public override Item2 Execute(Item item)
        {
            item.Process(GetType());

            return new Item2(item);
        }
    }
}
