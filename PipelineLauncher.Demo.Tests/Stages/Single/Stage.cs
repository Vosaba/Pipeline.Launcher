using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Stages;

namespace PipelineLauncher.Demo.Tests.Stages.Single
{
    public class Stage : Stage<Item>
    {
        public override Item Execute(Item item)
        {
            item.Process(GetType());

            return item;
        }
    }

    public class Stage_1 : Stage<Item>
    {
        public override Item Execute(Item item)
        {
            item.Process(GetType());

            return item;
        }
    }

    public class Stage_2 : Stage<Item>
    {
        public override Item Execute(Item item)
        {
            item.Process(GetType());

            return item;
        }
    }

    public class Stage_3 : Stage<Item>
    {
        public override Item Execute(Item item)
        {
            item.Process(GetType());

            return item;
        }
    }

    public class Stage_4 : Stage<Item>
    {
        public override Item Execute(Item item)
        {
            item.Process(GetType());

            return item;
        }
    }

    public class Stage_5 : Stage<Item>
    {
        public override Item Execute(Item item)
        {
            item.Process(GetType());

            return item;
        }
    }

    public class Stage_6 : Stage<Item>
    {
        public override Item Execute(Item item)
        {
            item.Process(GetType());

            return item;
        }
    }

    public class Stage_7 : Stage<Item>
    {
        public override Item Execute(Item item)
        {
            item.Process(GetType());

            return item;
        }
    }

    public class Stage_8 : Stage<Item>
    {
        public override Item Execute(Item item)
        {
            item.Process(GetType());

            return item;
        }
    }
}
