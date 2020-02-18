using System.Collections.Generic;
using System.Linq;

namespace PipelineLauncher.Demo.Tests.Items
{
    public class Item2
    {
        private Item _item;

        public Item2(Item item)
        {
            _item = item;
        }

        public Item GetItem() => _item;

        public override string ToString()
        {
            var values = _item.ProcessedBy.Select(x => $"{x.StageType.Name} : {x.ProcessId}").ToArray();
            return $"{nameof(Item2)}#{_item.Index}: {{ {string.Join(" } -> { ", values)} }};";
        }
    }
}
