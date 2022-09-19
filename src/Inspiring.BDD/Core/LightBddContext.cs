using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Inspiring.BDD.Core {
    public class LightBddContext : IScenarioContext {
        private static readonly AsyncLocal<IScenarioContext> _currentScenario = new AsyncLocal<IScenarioContext>();
        private static readonly AsyncLocal<MethodInfo> _currentScenarioMethod = new AsyncLocal<MethodInfo>();
        private readonly IScenarioRunner<NoContext> _runner;
        private readonly object _fixture;
        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        public static IScenarioContext CurrentScenario =>
            _currentScenario.Value ??
            throw new InvalidOperationException();

        public LightBddContext(object fixture, IScenarioRunner<NoContext> runner) {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
            _runner = runner ?? throw new ArgumentNullException(nameof(runner));
        }

        public void AddAsyncStep(string name, Func<Task> step) {
            if (step == null) throw new ArgumentNullException(nameof(step));
            _runner.AddAsyncStep(name, _ => step());
        }

        public void AddStep(string name, Action step) {
            if (step == null) throw new ArgumentNullException(nameof(step));
            _runner.AddStep(name, _ => step());
        }

        public void Use(IDisposable @object) {
            _disposables.Add(@object);
        }

        public void Dispose() {
            _disposables.ForEach(x => x.Dispose());
        }

        public async Task Run(params object?[] args) {
            if (_currentScenario.Value != null)
                throw new InvalidOperationException("There is already an active screnario.");

            _currentScenario.Value = this;

            try {
                MethodInfo backgroundMethod = _fixture.GetType()
                    .GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.GetCustomAttribute<BackgroundAttribute>() != null)
                    .SingleOrDefault();

                if (backgroundMethod != null)
                    backgroundMethod.Invoke(_fixture, null);

                if (args.Any()) {
                    _currentScenarioMethod.Value.Invoke(_fixture, args);
                } else {
                    InvokeWithDefaultParameterValues(_currentScenarioMethod.Value);
                }

                await _runner.RunAsync();
            } finally {
                _currentScenario.Value = null!;
            }
        }

        internal static void SetScenario(MethodInfo scenarioMethod) {
            if (_currentScenarioMethod.Value != null)
                throw new InvalidOperationException("Scenario method already set for the current async context.");

            _currentScenarioMethod.Value = scenarioMethod ?? throw new ArgumentNullException(nameof(scenarioMethod));
        }

        internal static void ClearScenario() {
            if (_currentScenarioMethod.Value == null)
                throw new InvalidOperationException("No scenario method is set for the current async context.");

            _currentScenarioMethod.Value = null!;
        }

        private void InvokeWithDefaultParameterValues(MethodInfo method) {
            object?[] args = GetParameterDefaultValues(method
                    .GetParameters()
                    .Select(x => x.ParameterType));

            method.Invoke(_fixture, args);
        }

        private object?[] GetParameterDefaultValues(IEnumerable<Type> types) {
            return types
                .Select(getDefaultValue)
                .ToArray();

            static object? getDefaultValue(Type type) =>
                type.IsValueType ?
                    Activator.CreateInstance(type) :
                    null;
        }
    }

}
