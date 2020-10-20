namespace PipelineLauncher.Extensions.Models
{
    public class WrappedInput<TInput, TData>
    {
        public WrappedInput(TInput input, TData data)
        {
            Input = input;
            Data = data;
        }

        public TInput Input { get; set; }

        public TData Data { get; set; }
    }
}
