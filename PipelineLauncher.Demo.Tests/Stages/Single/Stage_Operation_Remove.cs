using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Stages;

namespace PipelineLauncher.Demo.Tests.Stages.Single
{
    public class Stage_Operation_Remove : Stage<Item>
    {
        public override Item Execute(Item item)
        {
            item.Process(GetType());

            return Remove(item);
        }
    }
}
