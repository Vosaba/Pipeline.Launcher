using System;
using System.Collections.Generic;
using PipelineLauncher.Dataflow.Enumeration;

namespace PipelineLauncher.Dataflow
{
    public interface IExecutionBlock
    {
        ExecutionState State { get; }
        List<Exception> Exceptions { get; }
    }
}