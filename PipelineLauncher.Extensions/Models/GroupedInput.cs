namespace PipelineLauncher.Extensions.Models
{
    public class GroupedInput<TKey, TInput>
    {
        public GroupedInput(TKey key, TInput[] group)
        {
            Group = group;
            Key = key;
        }

        public TKey Key { get; set; }

        public TInput[] Group { get; set; }
    }
}