using System;
using System.Collections.Generic;

namespace PipelineLauncher.Jobs
{
    internal class LambdaJob<TInput, TOutput> : Job<TInput, TOutput>
    {
        private readonly Func<IEnumerable<TInput>, IEnumerable<TOutput>> _func;

        public LambdaJob(Func<IEnumerable<TInput>, IEnumerable<TOutput>> func)
        {
            _func = func;
        }

        public override IEnumerable<TOutput> Execute(TInput[] param)
        {
            return _func(param);
        }
    }
}