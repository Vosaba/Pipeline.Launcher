using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Stages;

namespace PipelineLauncher.Demo.Tests.Stages.Single
{
    public class Stage_Item_To_Item2: Stage<Item, Item2>
    {
        public override Item2 Execute(Item item)
        {
            item.Process(GetType());

            return new Item2(item);
        }
    }
}
