using PipelineLauncher.Demo.Tests.Fakes;
using PipelineLauncher.Demo.Tests.Stages;
using PipelineLauncher.Stages;
using PipelineLauncher.PipelineSetup;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using PipelineLauncher.Extensions;
using Xunit;
using Xunit.Abstractions;
using PipelineLauncher.Abstractions.Stages;

namespace PipelineLauncher.Demo.Tests.Modules
{
    public static class PipelineExtensions
    {
        public static IPipelineSetupOut<int> MssCall(this IPipelineSetupOut<Item> pipelineSetup, string someValue)
        {
            return pipelineSetup.Stage(e => e.GetHashCode());
        }
        
        public static IPipelineSetupOut<TOutput> TestStage<TStage, TInput, TOutput>(this IPipelineSetupOut<TInput> pipelineSetup) 
            where TStage : Stage<TInput, TOutput>
        {
            var t = pipelineSetup.AccessStageService();

            return pipelineSetup.Stage(t.GetStageInstance<TStage>());
        }
    }
    public class PipelineCreationMultipleTests : PipelineTestsBase
    {
        public IPipelineCreator PipelineCreator = new PipelineCreator();

        public PipelineCreationMultipleTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async void Pipeline_Creation_Multiple_AsyncStages()
        {
            //Test input 6 items
            List<int> input = MakeInputInt(2147483647/1000);

            CancellationTokenSource source = new CancellationTokenSource();
            //Configure stages

            var pipelineSetup = PipelineCreator
                    .WithCancellationToken(source.Token)
                    //.WithDiagnostic(e =>
                    //{
                    //    Output.WriteLine($"WD: {e.StageType.Name}: {e.State}: {e.RunningTime.TotalMilliseconds}: {e.Message}");
                    //})
                    .Stage(async (int item) =>
                    {
                        await Task.Delay(10, source.Token);
                        item++;
                        //item.Value += "1->";
                        //if (item.Value == ("Item#0->1->"))
                        //{
                        //    //await Task.Delay(1000);
                        //}      
                        return item;
                    })
                    //.Stage((item) =>
                    //{
                    //    //item.Value += "2->";
                    //    //if (item.Value == ("Item#0->1->2->"))
                    //    //{
                    //    //    //throw new Exception("Test exception");
                    //    //}

                    //    return item;
                    //})
                    //.Stage((item) =>
                    //{
                    //    //item.Value += "3->";
                    //    //if (item.Value.StartsWith("Item#0"))
                    //    //{
                    //    //    //throw new Exception("Test exception");
                    //    //}

                    //    //var t = DateTime.Now;
                    //    //item.Value += $"[{t.Second + "." + t.Millisecond}]->";

                    //    return item;
                    //})
                ;


            Stopwatch stopWatch = new Stopwatch();
            //var skippedItems = new List<Item>();
            //Make pipeline from stageSetup


            var pipeline = pipelineSetup.CreateAwaitable();

            //Task.Run(() =>
            //{
            //    Thread.Sleep(1000);
            //    source.Cancel();
            //});

            //pipeline.ExceptionItemsReceivedEvent += delegate(ExceptionItemsEventArgs args)
            //{
            //    //Output.WriteLine($"{args.StageName}: {args.Exception}");
            //};

            //source.CancelAfter(30);

            //run
            stopWatch.Start();
            var result = pipeline.Process(input).ToArray();

            stopWatch.Stop();

            PrintOutputAndTime(stopWatch.ElapsedMilliseconds, new [] {result.Length});
            stopWatch.Reset();

            stopWatch.Start();

            List<int> result2 = new List<int>();

            for (var index = 0; index < input.Count; index++)
            {
                await Task.Delay(10, source.Token);
                result2.Add(input[index]++);
            }

            stopWatch.Stop();

            PrintOutputAndTime(stopWatch.ElapsedMilliseconds, new[] { result2.Count });
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
        public void Pipeline_Creation_Multiple_Stages()
        {
            var t = Assembly.GetExecutingAssembly();

            var y = t.GetReferencedAssemblies();

            //Test input 6 items
            List<Item> input = MakeInput(2);

            CancellationTokenSource source = new CancellationTokenSource();

            var errorsCount = 0;
            //Configure stages
            var pipelineSetup = PipelineCreator
                .WithExceptionHandler(args =>
                {
                   // args.Retry();
                })
                //.WithToken(source.Token)
                //.WithDiagnostic(item =>
                //{
                //    if (item.State == DiagnosticState.Process)
                //    {
                //        Output.WriteLine($"{item.StageName}: '{string.Join("'; '", item.ItemsHashCode)}'");
                //    }
                //})
                //.Prepare<Item>()
                .Stage<Stage1, Item>()
                //.Stage((Item item, StageOption<Item, Item> option) =>
                //{
                //    if (item.Value.StartsWith("Item#0->"))
                //    {
                //        errorsCount++;
                //        //throw new Exception("lol");
                //        //return option.SkipTo<Stage4>(item);
                //    }

                //    return item;
                //})
                .BulkStage(new BulkStageStage2())
                .Broadcast(
                    (item => true, // => "Item#NEW->AsyncStage3->AsyncStage4->Stage4->AsyncStage1->",
                        branch1 => branch1
                            .Stage(x =>
                            {
                                x.Value += "111->";
                                return x;
                            })),
                    (item => true,
                        branch1 => branch1
                            .Stage((Item x, StageOption<Item, Item> stageOption) =>
                            {
                                //await Task.Delay(2000, source.Token);
                                if (x.Value.StartsWith("Item#0->"))
                                {
                                    return stageOption.SkipTo<Stage4>(x);
                                    //throw new Exception("lol");
                                    //return option.SkipTo<Stage4>(item);
                                }
                                x.Value += "222->";
                                return x;
                            })))
                .Stage(new Stage3())
                .Stage(new Stage4());
                //.Branch(
                //    (item => item.Value.StartsWith("Item#1->"),
                //        branch => branch
                //            .Stage<Stage2>()

            //    ),
            //    (item => true,
            //        branch => branch
            //            .Stage<Stage2>()
            //            .Stage<Stage2>()
            //            .Broadcast(
            //                (item => true, // => "Item#NEW->AsyncStage3->AsyncStage4->Stage4->AsyncStage1->",
            //                    branch1 => branch1
            //                        .Stage(x =>
            //                        {
            //                            x.Value += "111->";
            //                            return x;
            //                        })),
            //                (item => true,
            //                    branch1 => branch1
            //                        .Stage((Item x, StageOption<Item, Item> stageOption) =>
            //                        {

            //                            x.Value += "222->";
            //                            return x;
            //                        })))
            //    ))
            //.Delay(12000)
            //.BulkStage(new BulkStageStage3
            //.Stage((item) =>
            //{
            //    item.Value += "333->";
            //    return item;
            //})


            ////s.Stage(Task.FromResult)
            ////.BulkDelay(5000)

            //.Stage<Stage4>()
            //.Stage((Item item, StageOption<Item, Item> stageOption) =>
            //{

            //    if (item.Value.StartsWith("Item#0"))
            //    {
            //        //throw new Exception("Test exception");
            //    }

            //    var t = DateTime.Now;
            //    item.Value += $"[{t.Second + "." + t.Millisecond}]->";

            //    return item;
            //});
            //.Stage<Stage4>();//.ExtensionContext(extensionContext => extensionContext.MssCall(""));


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
                //args.Retry();
            };

            //source.CancelAfter(4900);

            //run
            stopWatch.Start();
            var result = pipeline
                .SetupCancellationToken(source.Token)
                //.SetupExceptionHandler(args =>
                //{
                //    args.Retry();
                //})
                .Process(input).ToArray();

            stopWatch.Stop();

            PrintOutputAndTime(stopWatch.ElapsedMilliseconds, result);
            stopWatch.Reset();

            //var pipeline2 = pipelineSetup.CreateAwaitable();


            return;
            //run
            stopWatch.Start();
            var result2 = pipeline.Process(result).ToArray();
            stopWatch.Stop();

            //Total time 24032
            PrintOutputAndTime(stopWatch.ElapsedMilliseconds, result2);
            stopWatch.Reset();
        }

        [Fact]
        public void Pipeline_Creation_Multiple_AsyncStages_Simple()
        {
            //Test input 6 items
            List<Item> input = MakeInput(8);

            CancellationTokenSource source = new CancellationTokenSource();
            //Configure stages
            var pipelineSetup = PipelineCreator
                .WithCancellationToken(source.Token)
                .Stage(async (Item item) =>
                {
                    await Task.Delay(1000);
                    return item;
                });
                

            Stopwatch stopWatch = new Stopwatch();

            var pipeline = pipelineSetup.CreateAwaitable();

            pipeline.ExceptionItemsReceivedEvent += delegate (ExceptionItemsEventArgs args)
            {
                args.Retry();
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
        public void Pipeline_Creation_Multiple_SyncStages()
        {
            //Test input 6 items
            List<Item> input = MakeInput(6);

            //Configure stages
            var stageSetup = new PipelineCreator(t => (IStage)Activator.CreateInstance(t))
                .BulkStage(new BulkStageStage1())
                .BulkStage(new BulkStageStage2())//, new Stage2Alternative())
                .BulkStage(new BulkStageStage4())
                .BulkStage(new BulkStageStage4());

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
        //public void Pipeline_Creation_Multiple_Async_and_SyncStages_generic()
        //{
        //    //Test input 6 items
        //    List<Item> input = MakeInput(6);

        //    //Configure stages
        //    var stageSetup = new PipelineFrom<Item>(new FakeServicesRegistry.StageService())
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
