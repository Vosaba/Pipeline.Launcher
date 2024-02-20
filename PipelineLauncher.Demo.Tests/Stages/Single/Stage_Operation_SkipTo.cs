using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Stages;

namespace PipelineLauncher.Demo.Tests.Stages.Single
{
    public class Stage_Operation_SkipTo : Stage<Item>
    {
        public override Item Execute(Item item)
        {
            item.Process(GetType());

            return SkipTo<Stage>(item);
        }
    }
}
