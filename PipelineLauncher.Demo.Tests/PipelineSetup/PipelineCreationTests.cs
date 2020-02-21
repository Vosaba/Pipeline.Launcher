using System;
using System.Threading;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.Stages;
using PipelineLauncher.Demo.Tests.Fakes;
using Xunit;

namespace PipelineLauncher.Demo.Tests.PipelineSetup
{
    public class PipelineCreationTests 
    {
        private IPipelineCreator _pipelineCreator;
        private readonly Func<Type, IStage> _stageResolveFunc;

        public PipelineCreationTests()
        {
            _stageResolveFunc = x => (IStage) Activator.CreateInstance(x);
        }

        [Fact]
        public void Pipeline_Creation_With_Overloads()
        {
            _pipelineCreator = new PipelineCreator();

            _pipelineCreator = new PipelineCreator(new StageServiceFake());

            _pipelineCreator = new PipelineCreator(_stageResolveFunc);
        }

        [Fact]
        public void Pipeline_Creation_With_Bounded_Context()
        {
            _pipelineCreator = new PipelineCreator();

            _pipelineCreator = _pipelineCreator.WithStageService(new StageServiceFake()); // IPipelineCreator

            _pipelineCreator = _pipelineCreator.WithStageService(_stageResolveFunc); // IPipelineCreator

            _pipelineCreator = _pipelineCreator
                .UseDefaultServiceResolver(true); // IPipelineCreator
                //.WithCancellationToken(CancellationToken.None); // IPipelineCreator
        }
    }
}
