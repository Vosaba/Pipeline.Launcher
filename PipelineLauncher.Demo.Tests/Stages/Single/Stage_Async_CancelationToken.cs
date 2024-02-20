using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Stages;

namespace PipelineLauncher.Demo.Tests.Stages.Single
{
    public class Stage_Async_CancelationToken : Stage<Item>
    {
        public override async Task<Item> ExecuteAsync(Item item, CancellationToken cancellationToken)
        {
            await Task.Delay(1000, cancellationToken);
            item.Process(GetType());

            return item;
        }
    }
}
