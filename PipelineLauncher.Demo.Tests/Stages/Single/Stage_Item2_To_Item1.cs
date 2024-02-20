using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Stages;

namespace PipelineLauncher.Demo.Tests.Stages.Single
{
    public class Stage_Item2_To_Item : Stage<Item2, Item>
    {
        public override Item Execute(Item2 item2)
        {
            var item = item2.GetItem();

            item.Process(GetType());

            return item;
        }
    }
}