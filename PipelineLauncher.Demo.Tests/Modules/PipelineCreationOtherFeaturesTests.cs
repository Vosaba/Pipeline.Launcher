using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using PipelineLauncher.Demo.Tests.Fakes;
using PipelineLauncher.Demo.Tests.Stages;
using PipelineLauncher.Pipelines;
using Xunit;
using Xunit.Abstractions;

namespace PipelineLauncher.Demo.Tests.Modules
{
    public class PipelineCreationOtherFeaturesTests : PipelineTestsBase
    {
        public PipelineCreationOtherFeaturesTests(ITestOutputHelper output)
            : base(output){}


        [Fact]
        public async void Pipeline_Creation_1()
        {
            //Test input 6 items
            List<Item> input = MakeInput(6);

            //Configure stages
            var stageSetup = new PipelineFrom<Item>(new FakeServicesRegistry.JobService())
                .AsyncStage<AsyncStage1_Filter>()
                .AsyncStage<AsyncStage2>()
                .Stage<Stage3>()
                .AsyncStage<AsyncStage4>();

            //Make pipeline from stageSetup
            var pipeline = stageSetup.From<Item>();
            //reusable stages config
            //var pipeline2 = new PipelineFrom<Item>(stageSetup);

            //run
            var result = pipeline.Run(input);
            //async
            //var result2 = await pipeline2.RunAsync(input);

            PrintOutputAndTime(0, input);
        }


        [Fact]
        public void Pipeline_Creation_2()
        {
            //Test input 6 items
            List<Item> input = MakeInput(6);

            //Configure stages
            var stageSetup = new PipelineFrom<Item>(new FakeServicesRegistry.JobService())
                .AsyncStage<AsyncStage1_Filter>()
                .AsyncStage<AsyncStage2>()
                .AsyncStage(item =>
                {
                    Thread.Sleep(500);
                    item.Value = item.Value + "AsyncLambda->";

                    item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);
                    return item;
                })
                .Stage<Stage3>()
                .Stage(items =>
                {
                    foreach (var item in items)
                    {
                        item.Value = item.Value + "SyncLambda->";

                        item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);
                    }
                    
                    return items;
                })
                .AsyncStage<AsyncStage4>();

            //Make pipeline from stageSetup
            var pipeline = stageSetup.From<Item>();

            //run
            var result = pipeline.Run(input);

            PrintOutputAndTime(0, input);
        }


        [Fact]
        public async void Pipeline_Creation_3()
        {
            //Configure stages
            var stageSetup = new PipelineFrom<string>()
                .AsyncStage(int.Parse)
                .AsyncStage(item => ++item)
                .Stage(items => items.Select(e => "result = " + e.ToString()));

            //Make pipeline from stageSetup
            var pipeline = stageSetup.From<string>();

            //run
            var result = await pipeline.RunAsync(new List<string>(){ "1", "2"});
        }
    }
}
