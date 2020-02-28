using PipelineLauncher.Demo.Tests.Extensions;
using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Demo.Tests.Stages.Single;
using PipelineLauncher.Exceptions;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace PipelineLauncher.Demo.Tests.PipelineTest.PipelineRunner
{

    public class FeatureStagesTests : PipelineTestBase
    {
        public FeatureStagesTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void Feature_Exception_Retry_Awaitable_Stages_WrongConfiguration()
        {
            // Test input 6 items
            List<Item> items = MakeItemsInput(6);

            // Configure stages
            var pipelineSetup = PipelineCreator
                .Stage<StageS, Item>()
                .Stage(item =>
                {
                    item.Process(GetType());

                    if (item.Index == 2)
                    {
                        throw new Exception($"{item.Name} throw exception");
                    }

                    return item;
                })
                .Stage<StageS1>();

            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup.CreateAwaitable();

            pipelineRunner.ExceptionItemsReceivedEvent += (args) =>
            {
                var item = args.Items[0];

                WriteSeparator();
                WriteLine($"{item} with exception {args.Exception.Message}");
                WriteSeparator();

                Assert.Throws<PipelineUsageException>(() => args.Retry());
            };

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items);
        }

        [Fact]
        public void Feature_Exception_Retry_Awaitable_Stages()
        {
            // Test input 6 items
            List<Item> items = MakeItemsInput(6);

            // Error occurred count
            var errorsCount = 0;

            // Configure stages
            var pipelineSetup = PipelineCreator
                .Stage<StageS, Item>()
                .Stage(item =>
                {
                    item.Process(GetType());

                    if (item.Index == 2 && errorsCount++ < 1)
                    {
                        throw new Exception($"{item.Name} throw exception: #'{errorsCount}'");
                    }

                    return item;
                })
                .Stage<StageS1>();

            // Make pipeline from stageSetup
            var pipelineRunner = pipelineSetup.CreateAwaitable();

            pipelineRunner.ExceptionItemsReceivedEvent += (args) =>
            {
                var item = args.Items[0];

                WriteSeparator();
                WriteLine($"{item} with exception {args.Exception.Message}");
                WriteSeparator();
            };

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items);
        }

        [Fact]
        public void Merge_Pipelines()
        {
            // Test input 6 items
            List<Item> items = MakeItemsInput(6);

            // Configure stages for the first PipelineSetup
            var pipelineSetup = PipelineCreator
                .Stage<StageS, Item>()
                .Stage<StageS1>();

            // Configure stages for the second PipelineSetup
            var pipelineSetup2 = PipelineCreator
                .Stage<StageS2, Item>()
                .Stage<StageS3>();

            // Configure stages for the third PipelineSetup
            var pipelineSetup3 = PipelineCreator
                .Stage<StageS4, Item>()
                .Stage<StageS5>();

            // Merge all pipelines setup 
            var mergedPipelineSetup = pipelineSetup.MergeWith(pipelineSetup2).MergeWith(pipelineSetup3);


            // Make pipeline from mergedPipelineSetup
            var pipelineRunner = mergedPipelineSetup.CreateAwaitable();

            // Process items and print result
            (this, pipelineRunner).ProcessAndPrintResults(items);


            // Make pipeline from first PipelineSetup
            var pipelineRunner1 = pipelineSetup.CreateAwaitable();

            // Process items and print result
            (this, pipelineRunner1).ProcessAndPrintResults(items);


            // Make pipeline from first PipelineSetup
            var pipelineRunner2 = pipelineSetup2.CreateAwaitable();

            // Process items and print result
           //(this, pipelineRunner2).ProcessAndPrintResults(items);
        }
    }
}
