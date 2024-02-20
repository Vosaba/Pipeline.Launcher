using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using PipelineLauncher.Demo.Tests.Items;
using Xunit.Abstractions;

namespace PipelineLauncher.Demo.Tests.PipelineTest
{
    public abstract class PipelineTestBase
    {
        private const string Separator = "--------------------";
        private readonly ITestOutputHelper _output;

        protected virtual IPipelineCreator PipelineCreator { get; } = new PipelineCreator();

        protected PipelineTestBase(ITestOutputHelper output)
        {
            _output = output;
        }

        protected List<Item> MakeItemsInput(int count)
            => MakeInput(count, index => new Item(index));

        protected List<int> MakeNumbersInput(int count)
        => MakeInput(count, index => index);

        protected List<TInput> MakeInput<TInput>(int count, Func<int, TInput> itemInitializer)
        {
            var input = new List<TInput>();

            for (int i = 0; i < count; i++)
            {
                input.Add(itemInitializer(i));
            }

            return input;
        }

        public Stopwatch StartTimer()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            return stopWatch;
        }

        public long StopTimerAndReturnElapsed(Stopwatch stopWatch)
        {
            stopWatch.Stop();
            return stopWatch.ElapsedMilliseconds;
        }

        public void StopTimerAndPrintResult(IEnumerable items, Stopwatch stopWatch)
        {
            StopTimerAndPrintElapsedTime(stopWatch);

            PrintResult(items);
        }

        public void PrintResult(IEnumerable items)
        {
            foreach (var item in items)
            {
                WriteLine(item);
            }

            WriteSeparator();
        }

        public void PrintProcessed(object item)
        {
            WriteLine(item);
        }

        public void StopTimerAndPrintElapsedTime(Stopwatch stopWatch)
        {
            var elapsedMilliseconds = StopTimerAndReturnElapsed(stopWatch);

            WriteSeparator();
            WriteLine($"Total elapsed milliseconds: {elapsedMilliseconds}");
            WriteSeparator();
        }

        protected void WriteSeparator() => WriteLine(Separator);
        protected void WriteLine(object value) => WriteLine(value.ToString());
        protected void WriteLine(string value) => _output.WriteLine(value);
    }
}
