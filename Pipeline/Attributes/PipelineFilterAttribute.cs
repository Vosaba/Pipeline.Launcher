using System;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.PipelineJobs;

namespace PipelineLauncher.Attributes
{
    public abstract class FilterService
    {
        internal abstract PipelineFilterResult Filter(object param);
    }

    public abstract class FilterService<TInput> : FilterService
    {
        public abstract PipelineFilterResult Filter(TInput param);

        internal override PipelineFilterResult Filter(object param)
        {
            return Filter((TInput) param);
        }

        public PipelineFilterResult Keep()
        {
            return new KeepResult();
        }

        public PipelineFilterResult Remove()
        {
            return new RemoveResult();
        }

        public PipelineFilterResult Skip()
        {
            return new SkipResult();
        }

        public PipelineFilterResult SkipTo<TSkipToJob>() where TSkipToJob : PipelineJob<TInput>
        {
            return new SkipToResult(typeof(TSkipToJob));
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

        public PipelineFilterResult Execute(object param)
        {
            return _filter.Filter(param);
        }
    }
}
