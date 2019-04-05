using PipelineLauncher.Jobs;
using PipelineLauncher.Stages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Attributes;
using PipelineLauncher.PipelineJobs;
using PipelineLauncher.Pipelines;
using PipelineLauncher.Abstractions.Services;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
// ReSharper disable ClassNeverInstantiated.Local

namespace PipelineLauncher.Tests
{
    public class IPipeline_Tests
    {
        private readonly ITestOutputHelper output;

        public IPipeline_Tests(ITestOutputHelper output)
        {
            this.output = output;
        }

        public class CustomPipelineFilter : FilterService<Item>
        {
            public override PipelineFilterResult Filter(Item t)
            {
                Thread.Sleep(300);
                t.Value = t.Value + "FILTER->";

                t.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);

                if (t.Value == "#1->FILTER->")
                {
                    return Remove();

                }
                if (t.Value == "#3->FILTER->")
                {
                    return SkipTo<Stage4>();
                }

                return Keep();
            }
        }

        //[PipelineFilter(typeof(CustomPipelineFilter))]



        public class Item
        {
            public string Value { get; set; }
            public List<int> ProcessedBy { get; set; }
            public Item(string value)
            {
                Value = value;
                ProcessedBy = new List<int>();
            }

            public override string ToString()
            {
                return $"Processed by:  '{{{string.Join("}, {", ProcessedBy.ToArray())}}}'; Result: '{Value}'";
            }
        }

        public class Stage1 : Job<Item>
        {
            public override IEnumerable<Item> Execute(Item[] items)
            {
                foreach (var item in items)
                {
                    item.Value = item.Value + "1->";
                    Thread.Sleep(1000);

                    item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);
                }

                return items;
            }

            public override String ToString()
            {
                return "Stage1";
            }
        }

        public class Stage2 : AsyncJob<Item>
        {
            public override Item Execute(Item input)
            {
                input.Value = input.Value + "2->";
                Thread.Sleep(1000);

                input.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);

                return input;
            }

            public override bool Condition(Item item)
            {
                return item.Value != "Item#0->1->";
            }

            public override String ToString()
            {
                return "Stage2";
            }
        }

        public class Stage2Analog : AsyncJob<Item>
        {
            public override Item Execute(Item input)
            {
                input.Value = input.Value + "2analog->";
                Thread.Sleep(1000);

                input.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);

                return input;
            }

            public override bool Condition(Item item)
            {
                return item.Value == "Item#0->1->";
            }

            public override String ToString()
            {
                return "Stage2Analog";
            }
        }

        public class Stage3 : AsyncJob<Item, string>
        {
            public override string Execute(Item item)
            {
                //foreach (var item in items)
                {
                    item.Value = item.Value + "3->";
                    Thread.Sleep(1000);

                    item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);
                }

                return item.Value; //.Select(e=>e.Value);
            }

            public override bool Condition(Item input) => input.Value!= "Item#0->1->2->" && input.Value != "Item#1->1->2->" && input.Value != "Item#2->1->2->";

            public override string ToString()
            {
                return "Stage3";
            }
        }

        public class Stage3Analog : AsyncJob<Item, string>
        {
            public override string Execute(Item item)
            {
                //foreach (var item in items)
                {
                    item.Value = item.Value + "3analog->";
                    Thread.Sleep(1000);

                    item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);
                }

                return item.Value;//.Select(e => e.Value);
            }

            public override bool Condition(Item input)
            {
                return input.Value == "Item#0->1->2->" || input.Value == "Item#1->1->2->" || input.Value == "Item#2->1->2->";
            }

            public override string ToString()
            {
                return "Stage3Analog";
            }
        }

        public class Stage4 : Job<Item, string>
        {
            public override IEnumerable<string> Execute(Item[] items)
            {
                foreach (var item in items)
                {
                    item.Value = item.Value + "4->";
                    Thread.Sleep(1000);

                    item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);
                }

                return items.Select(e => e.Value);
            }
        }

        public class Stage4string : AsyncJob<string>
        {
            public override string Execute(string item)
            {
                //for (var index = 0; index < items.Length; index++)
                //{
                //    var item = items[index];
                //    item = item + "4->";
                //    Thread.Sleep(1000);
                //}

                 var t = item+ "4->";
                return t;
            }
        }


        public class JobService : IJobService
        {
            public TPipelineJob GetJobInstance<TPipelineJob>() where TPipelineJob : IPipelineJob
            {
                return (TPipelineJob)Activator.CreateInstance(typeof(TPipelineJob));
            }
        }



        [Fact]
        public async Task IPipeline_Assert_Creation()
        {
            //Test input 6 items
            List<Item> input = MakeInput(6);

            //Configure stages
            var stageSetup = new PipelineFrom<Item>(new JobService())
                .Stage(new Stage1())
                //.AsyncStage(new Stage2())
                //.Stage<Stage3>()
                //.Stage(new Stage4())
                .Branch(
                    (root => root.AsyncStage<Stage3Analog, string>(), item => item.Value.StartsWith("Item#0")),
                    //.AsyncStage<Stage4string>(),
                    //.Stage<Stage4string>()
                    //.Stage<Stage4string>(),
                    (root => root.AsyncStage<Stage2>()
                        .AsyncStage<Stage3, string>()
                        .AsyncStage<Stage4string>(), item => !item.Value.StartsWith("Item#0"))
                //.AsyncStage<Stage4string>()
                )
                .AsyncStage(( e) => e + "->5")
                .AsyncStage<Stage4string>();

            Stopwatch stopWatch = new Stopwatch();

            //Make pipeline from stageSetup
            var pipeline = stageSetup.From<Item>();

            //run
            stopWatch.Start();
            var result = await pipeline.RunAsync(input);
            stopWatch.Stop();

            var time = stopWatch.ElapsedMilliseconds.ToString();

            output.WriteLine("--------------------");
            output.WriteLine($"Elapsed milliseconds: {time}");
            output.WriteLine("--------------------");
            foreach (var param in input)
            {
                output.WriteLine(param.ToString());
            }
        }


        [Fact]
        public async void IPipeline_Assert_Creation_WithLambdaAsyncJob()
        {
            var cancel = new CancellationTokenSource();

            var stageSetup = new PipelineFrom<(bool isHold, Item item)>(new JobService())
                .AsyncStage(i => i.item)
                .Stage<Stage1, Item>()
                .AsyncStage(item =>
                {
                    item.Value = item.Value + "AsyncLambda->";

                    item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);
                    return item;
                })
                .AsyncStage<Stage2>()
                .AsyncStage(item =>
                {
                    item.Value = item.Value + "Lambda->";
                    Thread.Sleep(500);

                    item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);
                    return item;
                })
                .AsyncStage(new Stage3())
                ;



            var input = MakeInput(6);

            Stopwatch stopWatch = new Stopwatch();


            var pipeline1 = stageSetup.From<(bool isHold, Item item)>();

            //var pipeline2 = PipelineFrom<Test>.To<int>(stageSetup);
            stopWatch.Start();
            var result1 = await pipeline1.RunAsync(input.Select(e => (isHold: true, item: e)));

            //var result2 = await pipeline1.Run(input);

            stopWatch.Stop();
            var time = stopWatch.ElapsedMilliseconds.ToString();

            output.WriteLine("--------------------");
            output.WriteLine($"Elapsed milliseconds: {time}");
            output.WriteLine("--------------------");
            foreach (var param in input)
            {
                output.WriteLine(param.ToString());
            }



        }

        public List<Item> MakeInput(int count)
        {
            var input = new List<Item>();

            for (int i = 0; i < count; i++)
            {
                input.Add(new Item($"Item#{i}->"));
            }

            return input;
        }

        [Fact]
        public void IPipeline_Assert_Creation_WithLambda()
        {
            string h = "";

            var setup = new Pipelines.PipelineFrom<string>(null)
                .AsyncStage((string i) => int.Parse(i))
                .AsyncStage(i => i / Math.PI)
                .Stage(i => i.ToString());

            //var pipeline = Pipeline.Create<String>(setup);

            var param = new List<String>();

            for (int i = 0; i < 1000; i++)
            {
                param.Add("" + i);
            }

            //pipeline.Run(param);
        }

    }
}
