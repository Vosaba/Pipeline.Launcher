using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Configurations;
using PipelineLauncher.Jobs;

namespace PipelineLauncher.Demo.Tests.Stages
{
    public class BulkStage1 : Bulk<Item>
    {
        public override IEnumerable<Item> Execute(IEnumerable<Item> items)
        {
            foreach (var item in items)
            {
                item.Value = item.Value + "Stage1->";
                Thread.Sleep(1000);

                item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);
            }

            return items;
        }

        public override String ToString()
        {
            return "Stage1";
        }
    }

    public class BulkStage2 : Bulk<Item>
    {
        public override IEnumerable<Item> Execute(IEnumerable<Item> items)
        {
            foreach (var item in items)
            {
                item.Value = item.Value + "Stage2->";
                Thread.Sleep(1000);

                item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);
            }

            return items;
        }

        public override bool Condition(Item input)
        {
            return input.Value.StartsWith("Item#0") || input.Value.StartsWith("Item#1");
        }

        public override String ToString()
        {
            return "Stage2";
        }
    }

    public class BulkStage2Alternative : Bulk<Item>
    {
        public override IEnumerable<Item> Execute(IEnumerable<Item> items)
        {
            foreach (var item in items)
            {
                item.Value = item.Value + "Stage2Alternative->";
                Thread.Sleep(1000);

                item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);
            }

            return items;
        }

        public override bool Condition(Item input)
        {
            return !input.Value.StartsWith("Item#0") && !input.Value.StartsWith("Item#1");
        }

        public override String ToString()
        {
            return "Stage2Alternative";
        }
    }

    public class BulkStage3 : Bulk<Item>
    {
        public override async Task<IEnumerable<Item>> ExecuteAsync(IEnumerable<Item> items, CancellationToken token)
        {
            foreach (var item in items)
            {
                if (!token.IsCancellationRequested)
                {
                    await Task.Delay(1000);
                    item.Value = item.Value + "Stage3->";

                    item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);
                }
            }

            return items;
        }
        public override string ToString()
        {
            return "Stage3";
        }
    }

    public class BulkStage4 : Bulk<Item>
    {
        public override IEnumerable<Item> Execute(IEnumerable<Item> items)
        {
            foreach (var item in items)
            {
                item.Value = item.Value + "Stage4->";
                Thread.Sleep(1000);

                item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);
            }

            return items;
        }

        public override string ToString()
        {
            return "Stage4";
        }

        public override BulkJobConfiguration Configuration => new BulkJobConfiguration {
            BatchItemsCount = 10,
            BatchItemsTimeOut = 3000
        };
    }

    

    public class BulkIntStage : Bulk<int>
    {
        public override IEnumerable<int> Execute(IEnumerable<int> items)
        {
            return items;
        }

        public override string ToString()
        {
            return "Stage4";
        }
    }

    public class BulkStage_Item_To_String : Bulk<Item, string>
    {
        public override IEnumerable<string> Execute(IEnumerable<Item> items)
        {
            foreach (var item in items)
            {
                item.Value = item.Value + "Stage_Item_To_String->";
                Thread.Sleep(1000);

                item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);
            }

            return items.Select(e=> e.Value);
        }

        public override string ToString()
        {
            return "Stage_Item_To_String";
        }
    }

    public class BulkStage_String_To_Object : Bulk<string, object>
    {
        public override IEnumerable<object> Execute(IEnumerable<string> items)
        {
            return items.Select(e => new object());
        }

        public override string ToString()
        {
            return "Stage_String_To_Object";
        }
    }
}
