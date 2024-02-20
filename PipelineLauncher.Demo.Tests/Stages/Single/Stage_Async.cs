using System.Threading.Tasks;
using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Stages;

namespace PipelineLauncher.Demo.Tests.Stages.Single
{
    public class Stage_Async : Stage<Item>
    {
        public override async Task<Item> ExecuteAsync(Item item)
        {
            await Task.Delay(1000);
            item.Process(GetType());

            return item;
        }
    }
}
