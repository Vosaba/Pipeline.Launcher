using System;
using PipelineLauncher.Abstractions.Pipeline;

namespace PipelineLauncher.Attributes
{
    public abstract class FilterService
    {
        internal abstract PipelineFilterResult InternalPerform(object param);
    }

    public abstract class FilterService<TInput> : FilterService
    {
        public abstract PipelineFilterResult Perform(TInput param);

        internal override PipelineFilterResult InternalPerform(object param)
        {
            return Perform((TInput) param);
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

        public PipelineFilterResult SkipTo<TSkipToJob>() where TSkipToJob : IPipelineJob
        {
            return new SkipToResult(typeof(TSkipToJob));
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PipelineFilterAttribute : Attribute { 
    
        private readonly FilterService _filter;
        public PipelineFilterAttribute(Type filter)
        {
            if (filter.BaseType.BaseType != typeof(FilterService))
            {
                throw new Exception($"'{filter.Name}' not implements '{nameof(FilterService)}'");
            }

            _filter = (FilterService)Activator.CreateInstance(filter);
        }

        public PipelineFilterResult Perform(object param)
        {
            return _filter.InternalPerform(param);
        }
    }
}
