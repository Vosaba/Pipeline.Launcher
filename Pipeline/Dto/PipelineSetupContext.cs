using System;
using System.Threading;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Abstractions.PipelineEvents;
using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.DI;

namespace PipelineLauncher.Dto
{
    public class PipelineSetupContext
    {
        private IJobService _jobService;
        private PipelineJobContext _pipelineJobContext;

        public bool TryUseDefaultServiceResolver { get; set; } = true;

        public ActionsSet ActionsSet { get; private set; } = new ActionsSet();
        public CancellationToken CancellationToken { get; private set; }

        public IJobService JobService
        {
            get
            {
                if (_jobService == null)
                {
                    if (TryUseDefaultServiceResolver)
                    {
                        _jobService = new DefaultJobService();
                    }
                    else
                    {
                        throw new Exception($"'{nameof(IJobService)}' isn't provided, if you need to use Generic stage setups, provide service.");
                    }
                }

                return _jobService;
            }
        }

        public  PipelineSetupContext(IJobService jobService)
        {
            _jobService = jobService;
        }

        public PipelineSetupContext SetupJobService(IJobService jobService)
        {
            _jobService = jobService;
            return this;
        }

        public PipelineSetupContext SetupJobService(Func<Type, IPipelineJob> jobService)
        {
            _jobService = new DefaultLambdaJobService(jobService);
            return this;
        }

        public PipelineSetupContext SetupDiagnosticAction(Action<DiagnosticItem> diagnosticAction)
        {
            ActionsSet.SetDiagnosticAction(diagnosticAction);
            return this;
        }

        public PipelineSetupContext SetupCancellationToken(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
            return this;
        }

        public PipelineJobContext GetPipelineJobContext(Action reExecute)
        {
            ActionsSet.SetReExecuteAction(reExecute);

            if (_pipelineJobContext == null)
            {
                _pipelineJobContext = new PipelineJobContext(CancellationToken, ActionsSet);
            }


            return _pipelineJobContext;
        }

        
    }
}