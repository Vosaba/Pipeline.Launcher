using System;
using System.Threading;
using PipelineLauncher.Abstractions.Collections;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Attributes;
using PipelineLauncher.Collections;
using PipelineLauncher.Dto;

namespace PipelineLauncher.PipelineJobs
{
    public abstract class PipelineJob<TInput, TOutput> : IPipelineJob
    {
        protected PipelineJob()
        {
            InitOutput();
        }

        public IQueue<object> Output { get; private set; }

        public void InitOutput()
        {
            Output = new BlockingQueue<object>();
        }

        protected void NonParamResult(PipelineFilterResult result, object param, CancellationToken cancellationToken)
        {
            switch (result)
            {
                case KeepResult _:
                    Output.Add(param, cancellationToken);
                    break;
                case SkipResult _:
                    Output.Add(new StageSkipObject(param), cancellationToken);
                    break;
                case SkipToResult skipTo:
                    Output.Add(new StageSkipObject(param, skipTo.JobType), cancellationToken);
                    break;
            }
        }

        public Type AcceptedType => typeof(TInput);

        public virtual bool Condition(TInput input) => true;

    }
}