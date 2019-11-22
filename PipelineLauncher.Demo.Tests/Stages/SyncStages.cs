using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PipelineLauncher.Jobs;

namespace PipelineLauncher.Demo.Tests.Stages
{
    public class Stage1 : Job<Item>
    {
        public override IEnumerable<Item> Execute(Item[] items)
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

    public class Stage2 : Job<Item>
    {
        public override IEnumerable<Item> Execute(Item[] items)
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

    public class Stage2Alternative : Job<Item>
    {
        public override IEnumerable<Item> Execute(Item[] items)
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

    public class Stage3 : Job<Item>
    {
        public override IEnumerable<Item> Execute(Item[] items)
        {
            foreach (var item in items)
            {
                item.Value = item.Value + "Stage3->";
                Thread.Sleep(1000);

                item.ProcessedBy.Add(Thread.CurrentThread.ManagedThreadId);
            }

            return items;
        }
        public override string ToString()
        {
            return "Stage3";
        }
    }

    public class Stage4 : Job<Item>
    {
        public override IEnumerable<Item> Execute(Item[] items)
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
    }

    

    public class IntStage : Job<int>
    {
        public override IEnumerable<int> Execute(int[] items)
        {

            return items;
        }

        public override string ToString()
        {
            return "Stage4";
        }
    }

    public class Stage_Item_To_String : Job<Item, string>
    {
        public override IEnumerable<string> Execute(Item[] items)
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

    public class Stage_String_To_Object : Job<string, object>
    {
        public override IEnumerable<object> Execute(string[] items)
        {
            return items.Select(e => new object());
        }

        public override string ToString()
        {
            return "Stage_String_To_Object";
        }
    }
}
