using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Stages;

namespace PipelineLauncher.Demo.Tests.Stages.Single
{
    public class StageSOperationSkip : StageS<Item, string>
    {
        public override string Execute(Item item)
        {
            item.Process(GetType());

            return Skip(item);
        }
    }
}
