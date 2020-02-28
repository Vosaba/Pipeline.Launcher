using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Stages;
using System;

namespace PipelineLauncher.Demo.Tests.Stages.Single
{
    public class StageSException : StageS<Item>
    {
        public override Item Execute(Item item)
        {
            item.Process(GetType());

            throw new Exception("Test Exception");
        }
    }
}
