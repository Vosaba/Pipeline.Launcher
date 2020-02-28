using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Stages;

namespace PipelineLauncher.Demo.Tests.Stages.Single
{
    public class StageSOperationRemove : StageS<Item>
    {
        public override Item Execute(Item item)
        {
            item.Process(GetType());

            return Remove(item);
        }
    }
}
