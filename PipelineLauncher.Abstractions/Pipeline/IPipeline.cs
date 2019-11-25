using System;
using PipelineLauncher.Abstractions.Collections;

namespace PipelineLauncher.Abstractions.Pipeline
{
    public interface IPipelineJob
    {
        IQueue<object> Output { get; }

        Type AcceptedType { get; }

        void InitOutput();

        int MaxDegreeOfParallelism { get; }
    }
}
