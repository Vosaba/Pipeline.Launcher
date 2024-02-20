using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Stages;
using System;

namespace PipelineLauncher.Demo.Tests.Stages.Single
{
    public class Stage_Exception : Stage<Item>
    {
        public override Item Execute(Item item)
        {
            item.Process(GetType());

            throw new Exception("Test Exception");
        }
    }
}
