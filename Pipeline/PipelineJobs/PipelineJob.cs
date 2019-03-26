using System;
using System.Threading;
using PipelineLauncher.Abstractions.Collections;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Attributes;
using PipelineLauncher.Collections;
using PipelineLauncher.Dto;

namespace PipelineLauncher.PipelineJobs
{
    public abstract class PipelineJob<TInput>{
    }

    public abstract class PipelineJob<TInput, TOutput>: PipelineJob<TInput>, IPipelineJob
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

        protected void NonOutputResult(PipelineFilterResult result, object output, CancellationToken cancellationToken)
        {
            switch (result)
            {
                case RemoveResult _:
                    Output.Add(output, cancellationToken);
                    break;
                case SkipResult _:
                    Output.Add(new StageSkipObject(output), cancellationToken);
                    break;
                case SkipToResult skipTo:
                    Output.Add(new StageSkipObject(output, skipTo.JobType), cancellationToken);
                    break;
            }
        }

        public Type AcceptedType => typeof(TInput);

        public virtual bool Condition(TInput input) => true;

    }
}