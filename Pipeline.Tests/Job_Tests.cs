using PipelineLauncher.Jobs;
using System;
using System.Threading;
using FluentAssertions;
using Xunit;

namespace PipelineLauncher.Tests
{
    public class Job_Tests
    {
        class Simple : AsyncJob<Int32, String>
        {
            public  override string Perform(int param)
            {
                return "My number is : " + param;
            }
        }

        [Fact]
        public void SimpleJob_Assert_Output()
        {
            var job = new Simple();

            for (int i = 0; i < 100; i++)
            {
                Assert.Equal(job.Output.Count, i);

                job.InternalPerform(i, new CancellationToken(false));

                Assert.Equal(job.Output.Count, i+1);
            }

        }
    }
}
