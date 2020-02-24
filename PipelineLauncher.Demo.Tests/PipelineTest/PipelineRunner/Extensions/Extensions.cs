using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.PipelineSetup;

namespace PipelineLauncher.Demo.Tests.PipelineTest.PipelineRunner.Extensions
{
    public static class Extensions
    {
        public static IPipelineSetupSource<int> GetHashCode(this IPipelineSetupSource<Item> pipelineSetup, int uniqPostfix = 0)
        {
            return pipelineSetup.Stage(x => x.GetHashCode() + uniqPostfix);
        }

        public static IPipelineSetup<TInput, string> GetString<TInput>(this IPipelineSetup<TInput, int> pipelineSetup, string uniqPrefix, string uniqPostfix)
        {
            return pipelineSetup.Stage(x => $"{uniqPrefix}_{x}_{uniqPostfix}");
        }

        public static IPipelineSetup<TInput, TOutput> DoNothing<TInput, TOutput>(this IPipelineSetup<TInput, TOutput> pipelineSetup)
        {
            return pipelineSetup.Stage(x => x);
        }

        public static IPipelineSetup<TInput, Contextual<TOutput, TContext>> MakeContext<TInput, TOutput, TContext>(this IPipelineSetup<TInput, TOutput> pipelineSetup, TContext context)
        {
            return pipelineSetup.Stage(x => new Contextual<TOutput, TContext>(x, context));
        }
    }

    public class Contextual<TItem, TContext>
    {
        public TItem Item { get; set; }
        public TContext Context { get; set; }

        public Contextual(TItem item, TContext context)
        {
            Item = item;
            Context = context;
        }
    }
}
