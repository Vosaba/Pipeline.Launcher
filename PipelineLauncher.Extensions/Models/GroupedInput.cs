using System.Collections.Generic;

namespace PipelineLauncher.Extensions.Models
{
    public class GroupedInput<TKey, TInput>
    {
        public GroupedInput(TKey key, TInput[] input)
        {
            Input = input;
            Key = key;
        }

        public TKey Key { get; set; }

        public TInput[] Input { get; set; }
    }
}