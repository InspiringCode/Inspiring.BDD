using LightBDD.XUnit2;
using System;
using System.Threading.Tasks;

namespace Inspiring.BDD.Core {
    public class LightBddFeature : FeatureFixture, IDisposable {
        private readonly Lazy<LightBddContext> _scenario;

        protected IScenarioContext Scenario
            => _scenario.Value;

        public LightBddFeature()
            => _scenario = new Lazy<LightBddContext>(() => new LightBddContext(this, Runner.Integrate()));

        public virtual void Dispose() {
            if (_scenario.IsValueCreated)
                _scenario.Value.Dispose();
        }

        private Task Run()
            => Scenario.Run();

        private Task Run(object arg1)
            => Scenario.Run(arg1);

        private Task Run(object arg1, object arg2)
            => Scenario.Run(arg1, arg2);

        private Task Run(object arg1, object arg2, object arg3)
            => Scenario.Run(arg1, arg2, arg3);

        private Task Run(object arg1, object arg2, object arg3, object arg4)
            => Scenario.Run(arg1, arg2, arg3, arg4);

        private Task Run(object arg1, object arg2, object arg3, object arg4, object arg5)
            => Scenario.Run(arg1, arg2, arg3, arg4, arg5);
    }
}
