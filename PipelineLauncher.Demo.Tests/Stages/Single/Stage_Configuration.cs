using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Stages;

namespace PipelineLauncher.Demo.Tests.Stages.Single
{
    public class Stage_Configuration : Stage<Item>
    {
        public override Item Execute(Item item)
        {
            item.Process(GetType());

            return item;
        }

        public override StageConfiguration Configuration 
            => new StageConfiguration 
            {
                MaxDegreeOfParallelism = 2
            };
    }
}
