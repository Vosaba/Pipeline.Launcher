using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Stages;

namespace PipelineLauncher.Demo.Tests.Stages.Single
{
    public class StageSItem2ToItem : StageS<Item2, Item>
    {
        public override Item Execute(Item2 item2)
        {
            var item = item2.GetItem();

            item.Process(GetType());

            return item;
        }
    }
}