using System;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.PipelineJobs;

namespace PipelineLauncher.Attributes
{
    public abstract class FilterService
    {
        internal abstract PipelineItem<T> Filter<T>(object param);
    }

    public abstract class FilterService<TInput> : FilterService
    {
        public abstract PipelineItem<TInput> Filter(TInput param);

        internal override PipelineItem<TInputf> Filter<TInputf>(object param)
        {
           // return Filter((TInput) param);
           throw new Exception();
        }

        //public PipelineItem<TInput> Keep(TInput item)
        //{
        //    return new KeepItem<TInput>(item);
        //}

        //public PipelineItem<TInput> Remove(TInput item)
        //{
        //    return new RemoveItem<TInput>(item);
        //}

        public PipelineItem<TInput> Skip(TInput item)
        {
            return new SkipItem<TInput>(item);
        }

        public PipelineItem<TInput> SkipTo<TSkipToJob>(TInput item) where TSkipToJob : IPipelineJobIn<TInput>
        {
            return new SkipItemTill<TInput>(typeof(TSkipToJob), item);
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PipelineFilterAttribute : Attribute { 
    
        private readonly FilterService _filter;
        public PipelineFilterAttribute(Type filter)
        {
            if (filter.BaseType != null && (filter.BaseType != typeof(FilterService) && filter.BaseType.BaseType != typeof(FilterService)))
            {
                throw new Exception($"'{filter.Name}' not implements '{nameof(FilterService)}'");
            }

            _filter = (FilterService)Activator.CreateInstance(filter);
        }

        //public PipelineItem<> Execute(object param)
        //{
        //    return _filter.Filter(param);
        //}
    }
}
