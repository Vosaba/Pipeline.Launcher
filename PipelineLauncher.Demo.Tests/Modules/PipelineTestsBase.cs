using System.Collections.Generic;
using PipelineLauncher.Demo.Tests.Fakes;
using PipelineLauncher.Pipelines;
using Xunit.Abstractions;

namespace PipelineLauncher.Demo.Tests.Modules
{
    public class PipelineTestsBase
    {
        public readonly ITestOutputHelper Output;

        public PipelineTestsBase(ITestOutputHelper output)
        {
            this.Output = output;
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

        public List<int> MakeInputInt(int count)
        {
            var input = new List<int>();

            for (int i = 0; i < count; i++)
            {
                input.Add(i);
            }

            return input;
        }

        public void PrintOutputAndTime<T>(long time, IEnumerable<T> items)
        {
            Output.WriteLine("--------------------");
            Output.WriteLine($"Elapsed milliseconds: {time}");
            Output.WriteLine("--------------------");
            foreach (var param in items)
            {
                Output.WriteLine(param.ToString());
            }
        }
    }
}