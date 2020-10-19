using PipelineLauncher.Extensions.Models;
using PipelineLauncher.PipelineSetup;

namespace PipelineLauncher.Extensions.PipelineSetup
{
    public static class PipelineSetup
    {
        public static IPipelineSetup<TPipelineInput, (TPipelineOutput input, TData data)> WrapInputAsTuple<TPipelineInput, TPipelineOutput, TData>(this IPipelineSetup<TPipelineInput, TPipelineOutput> pipelineSetup, TData data) =>
            pipelineSetup.Stage(input => (input, data));

        public static IPipelineSetup<TPipelineInput, WrappedInput<TPipelineOutput, TData>> WrapInput<TPipelineInput, TPipelineOutput, TData>(this IPipelineSetup<TPipelineInput, TPipelineOutput> pipelineSetup, TData data) =>
            pipelineSetup.Stage(input => new WrappedInput<TPipelineOutput, TData>(input, data));

        public static IPipelineSetup<TPipelineInput, TPipelineOutput> UnwrapInput<TPipelineInput, TPipelineOutput, TData>(this IPipelineSetup<TPipelineInput, (TPipelineOutput input, TData data)> pipelineSetup) =>
            pipelineSetup.Stage(wrappedInput => wrappedInput.input);

        public static IPipelineSetup<TPipelineInput, TPipelineOutput> UnwrapInput<TPipelineInput, TPipelineOutput, TData>(this IPipelineSetup<TPipelineInput, WrappedInput<TPipelineOutput, TData>> pipelineSetup) =>
            pipelineSetup.Stage(wrappedInput => wrappedInput.Input);
    }
}
