using PipelineLauncher.Abstractions.Configurations;
using PipelineLauncher.Demo.Tests.Fakes;
using PipelineLauncher.Demo.Tests.Stages;
using PipelineLauncher.Dto;
using PipelineLauncher.Extensions;
using PipelineLauncher.Jobs;
using PipelineLauncher.Pipelines;
using PipelineLauncher.PipelineSetup;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.PipelineEvents;
using Xunit;
using Xunit.Abstractions;

namespace PipelineLauncher.Demo.Tests.Modules
{
    public static class PipelineExtensions
    {
        public static IPipelineSetupOut<int> MssCall(this IPipelineSetupOut<Item> pipelineSetup, string someValue)
        {
            return pipelineSetup.Stage(e => e.GetHashCode());
        }

        public static IPipelineSetupOut<TOutput> TestStage<TJob, TInput, TOutput>(this IPipelineSetupOut<TInput> pipelineSetup) 
            where TJob : Job<TInput, TOutput>
        {
            var t = pipelineSetup.AccessJobService();

            return pipelineSetup.Stage(t.GetJobInstance<TJob>());
        }
    }
    public class PipelineCreationMultipleTests : PipelineTestsBase
    {
        public IPipelineCreator PipelineCreator = new PipelineCreator();

        public PipelineCreationMultipleTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public void Pipeline_Creation_Multiple_AsyncJobs()
        {
            //Test input 6 items
            List<Item> input = MakeInput(2);

            CancellationTokenSource source = new CancellationTokenSource();
            //Configure stages

            var pipelineSetup = PipelineCreator
                    .WithToken(source.Token)
                    //.WithDiagnostic(e =>
                    //{
                    //    Output.WriteLine($"WD: {e.StageType.Name}: {e.State}: {e.RunningTime.TotalMilliseconds}: {e.Message}");
                    //})
                    .Stage((Item item) =>
                    {
                        item.Value += "1->";
                        if (item.Value == ("Item#0->1->"))
                        {
                            //await Task.Delay(1000);
                        }      
                        return item;
                    })
                    .Stage((Item item) =>
                    {
                        item.Value += "2->";
                        if (item.Value == ("Item#0->1->2->"))
                        {
                            //throw new Exception("Test exception");
                        }

                        return item;
                    })
                    .Stage((Item item, StageOption<Item, Item> stageOption) =>
                    {
                        item.Value += "3->";
                        if (item.Value.StartsWith("Item#0"))
                        {
                            //throw new Exception("Test exception");
                        }

                        var t = DateTime.Now;
                        //item.Value += $"[{t.Second + "." + t.Millisecond}]->";

                        return item;
                    })
                ;


            Stopwatch stopWatch = new Stopwatch();
            //var skippedItems = new List<Item>();
            //Make pipeline from stageSetup


            var pipeline = pipelineSetup.CreateAwaitable(new AwaitablePipelineConfig { ThrowExceptionOccured=true});

            //Task.Run(() =>
            //{
            //    Thread.Sleep(1000);
            //    source.Cancel();
            //});

            pipeline.ExceptionItemsReceivedEvent += delegate(ExceptionItemsEventArgs args)
            {
                //Output.WriteLine($"{args.StageName}: {args.Exception}");
            };

            //source.CancelAfter(30);

            //run
            stopWatch.Start();
            var result = pipeline.Process(input).ToArray();

            stopWatch.Stop();

            PrintOutputAndTime(stopWatch.ElapsedMilliseconds,result);
            stopWatch.Reset();

            stopWatch.Start();
            var resul2t = pipeline.Process(result).ToArray();

            stopWatch.Stop();

            PrintOutputAndTime(stopWatch.ElapsedMilliseconds, resul2t);
            stopWatch.Reset();

            //var pipeline2 = pipelineSetup.CreateAwaitable();

            //run
            //stopWatch.Start();
            //var result2 = pipeline.Process(result).ToArray();
            //stopWatch.Stop();

            ////Total time 24032
            //PrintOutputAndTime(stopWatch.ElapsedMilliseconds, result2);
            ////stopWatch.Reset();

            //stopWatch.Start();
            //var result3 = pipeline.Process(result2).ToArray();
            //stopWatch.Stop();

            ////Total time 24032
            //PrintOutputAndTime(stopWatch.ElapsedMilliseconds, result3);
            //stopWatch.Reset();

            //stopWatch.Start();
            //var result4 = pipeline.Process(result3).ToArray();
            //stopWatch.Stop();

            ////Total time 24032
            //PrintOutputAndTime(stopWatch.ElapsedMilliseconds, result4);
            //stopWatch.Reset();

            //stopWatch.Start();
            //var result5 = pipeline.Process(result4).ToArray();
            //stopWatch.Stop();

            ////Total time 24032
            //PrintOutputAndTime(stopWatch.ElapsedMilliseconds, result5);
            //stopWatch.Reset();
            //PrintOutputAndTime(stopWatch.ElapsedMilliseconds, result);


            //stopWatch.Reset();
            //stopWatch.Start();
            //result = pipeline.Run(input);

            //while (result.Count() < 12)
            //{

            //}
            //stopWatch.Stop();


            ////Total time 24032
            //PrintOutputAndTime(stopWatch.ElapsedMilliseconds, input);


        }

        [Fact]
        public void Pipeline_Creation_Multiple_Jobs()
        {
            var t = Assembly.GetExecutingAssembly();

            var y = t.GetReferencedAssemblies();

            //Test input 6 items
            List<Item> input = MakeInput(5);

            CancellationTokenSource source = new CancellationTokenSource();

            //Configure stages
            var pipelineSetup = PipelineCreator
                .WithToken(source.Token)
                .Stage(new Stage1())
                .Stage<Stage1>()
                .Branch(
                    (item => item.Value == "Item#1->AsyncStage1->",
                        branch => branch
                            .Stage<Stage2>()
                            .BulkStage(items =>
                            {
                                var itemsArray = items.ToList();

                                itemsArray.Add(new Item("Item#NEW->"));

                                return itemsArray;
                            })
                    ),
                    (item => true,
                        branch => branch
                            .Stage<Stage2>()
                            .Stage<Stage2>()
                            .Broadcast(
                                (item => true, //=> "Item#NEW->AsyncStage3->AsyncStage4->Stage4->AsyncStage1->",
                                    branch1 => branch1
                                        .Stage(x =>
                                        {
                                            x.Value += "111111111111111111111111->";
                                            return x;
                                        })),
                                (item => true,
                                    branch1 => branch1
                                        .Stage(x =>
                                        {
                                            x.Value += "222222222222222222222222->";
                                            return x;
                                        })))
                    ))
                //.Delay(12000)
                //.BulkStage(new BulkJobStage3())
                .BulkStage((items) =>
                {
                    var count = items.Count();
                    return items;
                },
                    new BulkJobConfiguration()
                    {
                        BatchItemsCount = 2,
                        BatchItemsTimeOut = 4000
                    })
                .Stage<Stage4>()
                //s.Stage(Task.FromResult)
                //.BulkDelay(5000)
                .BulkStage((items) =>
                {
                    var count = items.Count();
                    return items;
                },
                    new BulkJobConfiguration()
                    {
                        BatchItemsCount = 5,
                        BatchItemsTimeOut = 4000
                    })
                .Stage((Item item, StageOption<Item, Item> stageOption) =>
                {

                    if (item.Value.StartsWith("Item#0"))
                    {
                        throw new Exception("Test exception");
                    }

                    var t = DateTime.Now;
                    item.Value += $"[{t.Second + "." + t.Millisecond}]->";

                    return item;
                });//.ExtensionContext(extensionContext => extensionContext.MssCall(""));


            Stopwatch stopWatch = new Stopwatch();
            //var skippedItems = new List<Item>();
            //Make pipeline from stageSetup
            //pipeline.SkippedItemReceivedEvent += delegate(SkippedItemEventArgs item) { skippedItems.Add(item.Item); };

            var pipeline = pipelineSetup.CreateAwaitable();

            //Task.Run(() =>
            //{
            //    Thread.Sleep(7000);
            //    //source.Cancel();
            //});

            pipeline.ExceptionItemsReceivedEvent += delegate (ExceptionItemsEventArgs args)
            {

            };

            //run
            stopWatch.Start();
            var result = pipeline.Process(input).ToArray();

            stopWatch.Stop();

            PrintOutputAndTime(stopWatch.ElapsedMilliseconds, result);
            stopWatch.Reset();

            //var pipeline2 = pipelineSetup.CreateAwaitable();

            //run
            stopWatch.Start();
            var result2 = pipeline.Process(result).ToArray();
            stopWatch.Stop();

            //Total time 24032
            PrintOutputAndTime(stopWatch.ElapsedMilliseconds, result2);
            stopWatch.Reset();
        }

        [Fact]
        public void Pipeline_Creation_Multiple_AsyncJobs_Simple()
        {
            //Test input 6 items
            List<Item> input = MakeInput(8);

            CancellationTokenSource source = new CancellationTokenSource();
            //Configure stages
            var pipelineSetup = PipelineCreator
                .WithToken(source.Token)
                .Stage(async (Item item) =>
                {
                    await Task.Delay(1000);
                    return item;
                });
                

            Stopwatch stopWatch = new Stopwatch();

            var pipeline = pipelineSetup.CreateAwaitable();

            pipeline.ExceptionItemsReceivedEvent += delegate (ExceptionItemsEventArgs items)
            {

            };

            //run
            stopWatch.Start();
            var result = pipeline.Process(input).ToArray();

            stopWatch.Stop();

            PrintOutputAndTime(stopWatch.ElapsedMilliseconds, result);
            stopWatch.Reset();

            //run
            stopWatch.Start();
            var result2 = pipeline.Process(result).ToArray();
            stopWatch.Stop();

            //Total time 24032
            PrintOutputAndTime(stopWatch.ElapsedMilliseconds, result2);
            stopWatch.Reset();
        }
        [Fact]
        public void Pipeline_Creation_Multiple_SyncJobs()
        {
            //Test input 6 items
            List<Item> input = MakeInput(6);

            //Configure stages
            var stageSetup = new PipelineCreator(new FakeServicesRegistry.JobService())
                .BulkStage(new BulkJobStage1())
                .BulkStage(new BulkJobStage2())//, new Stage2Alternative())
                .BulkStage(new BulkJobStage4())
                .BulkStage(new BulkJobStage4());

            Stopwatch stopWatch = new Stopwatch();

            //Make pipeline from stageSetup
            var pipeline = stageSetup.CreateAwaitable();

            //run
            stopWatch.Start();
            var result = pipeline.Process(input);
            stopWatch.Stop();

            //Total time 24032
            PrintOutputAndTime(stopWatch.ElapsedMilliseconds, result);
        }

        //[Fact]
        //public void Pipeline_Creation_Multiple_Async_and_SyncJobs_generic()
        //{
        //    //Test input 6 items
        //    List<Item> input = MakeInput(6);

        //    //Configure stages
        //    var stageSetup = new PipelineFrom<Item>(new FakeServicesRegistry.JobService())
        //        .Stage<Stage1>()
        //        .Stage<Stage2>() //, Stage2Alternative, Item>()
        //        .AsyncStage(item => item)
        //        .AsyncStage<AsyncStage2, AsyncStage2Alternative, Item>();

        //    Stopwatch stopWatch = new Stopwatch();

        //    //Make pipeline from stageSetup
        //    var pipeline = stageSetup.From<Item>();

        //    //run
        //    stopWatch.Start();
        //    IEnumerable<Item> result = pipeline.Run(input);
        //    stopWatch.Stop();

        //    //Total time 24032
        //    PrintOutputAndTime(stopWatch.ElapsedMilliseconds, input);
        //}
    }
}
