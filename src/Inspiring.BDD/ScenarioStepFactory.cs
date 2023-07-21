using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Inspiring.BDD {
    /// <summary>
    /// Adapts the <see cref="ScenarioStepFactory"/> to a specific underlying test framework.
    /// </summary>
    public interface IScenarioContext {
        void AddAsyncStep(string name, Func<Task> step);

        void AddStep(string name, Action step);

        /// <summary>
        /// Disposes the <paramref name="object"/> after the test has been run.
        /// </summary>
        void Use(IDisposable @object);

        /// <summary>
        /// Used by the infrastructure. Runs the current scenario with the given test parameters.
        /// </summary>
        Task Run(params object?[] args);
    }

    /// <summary>
    /// Used to define BDD steps in a fluent, compact and readable way.
    /// </summary>
    public class ScenarioStepFactory {
        private readonly IScenarioContext _scenario;
        private readonly string _prefix;

        public ScenarioStepFactory(IScenarioContext scenario, string prefix = "") {
            _scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
            _prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
        }

        public Delegate this[string stepText] {
            set => AddStep(
                stepText ?? throw new ArgumentNullException(nameof(stepText)),
                value ?? throw new ArgumentNullException(nameof(value)));
        }

        private void AddStep(string text, Delegate step) {
            if (!String.IsNullOrEmpty(_prefix))
                text = _prefix + text;

            // We let the DLR do the work of finding the correct overload and determining the
            // actual type parameters and so on (the most specific overload is called).
            ((dynamic)this).AddStepCore(text, (dynamic)step);
        }

        /// <summary>
        /// Adds a synchronous step without return value.
        /// </summary>
        private void AddStepCore(string text, Action s) {
            _scenario.AddStep(text, s);
        }

        /// <summary>
        /// Adds a synchronous step that returns a value.
        /// </summary>
        private void AddStepCore<T>(string text, Func<T> s) {
            _scenario.AddStep(text, () => {
                T result = s();
                if (result is IDisposable d)
                    _scenario.Use(d);
            });
        }

        /// <summary>
        /// Adds an async step that returns a value.
        /// </summary>
        private void AddStepCore<T>(string text, Func<Task<T>> s) {
            _scenario.AddAsyncStep(text, async () => {
                T result = await s();
                if (result is IDisposable d)
                    _scenario.Use(d);
            });
        }

        /// <summary>
        /// Adds an async step that does not return a value.
        /// </summary>
        private void AddStepCore(string text, Func<Task> s) {
            _scenario.AddAsyncStep(text, s);
        }

        /// <summary>
        /// Adds an async ValueTask step that returns a value.
        /// </summary>
        private void AddStepCore<T>(string text, Func<ValueTask<T>> s) {
            _scenario.AddAsyncStep(text, async () => {
                T result = await s();
                if (result is IDisposable d)
                    _scenario.Use(d);
            });
        }
    }
}
