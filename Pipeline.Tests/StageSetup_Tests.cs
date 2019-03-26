using PipelineLauncher.Jobs;
using PipelineLauncher.Stages;
using System;
using System.Threading.Tasks;
using FluentAssertions;
using PipelineLauncher.Pipelines;
using Xunit;

namespace PipelineLauncher.Tests
{
    public class StageSetup_Tests
    {
        class ParseFromString : AsyncJob<String, int>
        {
            public override int Execute(string input)
            {
                throw new NotImplementedException();
            }
        }

        class DivideByPI : AsyncJob<int, Double>
        {
            public override double Execute(int input)
            {
                throw new NotImplementedException();
            }
        }

        class Format : AsyncJob<Double, String>
        {
            public override string Execute(double input)
            {
                throw new NotImplementedException();
            }
        }

        private static StageSetup<double, string> ConfigStages()
        {
            var setup = new Pipelines.PipelineFrom<string>(null)
                .AsyncStage(new ParseFromString())
                .AsyncStage(new DivideByPI())
                .AsyncStage(new Format());

            return setup;
        }

        [Fact]
        public void StageSetup_Assert_Current()
        {
            var setup = ConfigStages();

            Assert.NotNull(setup.Current);
            Assert.NotNull(setup.Current.Next);
            //Assert.That(setup.Current.First, Is.Not.Null, "The Current.First should not be null");

        }

        [Fact]
        public void StageSetup_Assert_First()
        {
            var setup = ConfigStages();

            //var first = setup.Current.First;

            //Assert.That(first, Is.Not.Null);
            //Assert.That(first.Next, Is.Not.Null, "The First.Next should not be null");
            //Assert.That(first.Job, Is.Not.Null, "The First.Job should not be null");
            //Assert.That(first.First, Is.Not.Null, "The First.First should not be null");
        }

        [Fact]
        public void StageSetup_Assert_Stage_Count()
        {
            var setup = ConfigStages();

            //var first = setup.Current.First;

            //var current = first;
            //var i = 0;
            //while (current != null)
            //{
            //    ++i;
            //    current = current.Next;
            //}

            //Assert.That(i, Is.EqualTo(3));            
        }
    }
}
