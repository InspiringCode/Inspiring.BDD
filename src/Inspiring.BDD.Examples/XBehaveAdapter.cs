using Inspiring.BDD;
using Inspiring.BDD.Core;

namespace Xbehave {
    /// <inheritdoc/>
    /// <remarks>
    /// A dummy subtype to map the <see cref="Inspiring.BDD.ScenarioAttribute"/> into the XBehave
    /// namespace. If you are already on C# 10 you can remove this class and use global usings
    /// instead.
    /// </remarks>
    public class ScenarioAttribute : Inspiring.BDD.ScenarioAttribute { }

    /// <inheritdoc/>
    /// <remarks>
    /// A dummy subtype to map the <see cref="Inspiring.BDD.BackgroundAttribute"/> into the XBehave
    /// namespace. If you are already on C# 10 you can remove this class and use global usings
    /// instead.
    /// </remarks>
    public class BackgroundAttribute : Inspiring.BDD.BackgroundAttribute { }

    /// <summary>
    /// Provides the <see cref="x"/> methods of XBehave but creates LightBDD steps instead.
    /// </summary>
    public static class XBehaveAdapter {
        /// <summary>
        /// Defines a step in the current scenario.
        /// </summary>
        /// <param name="text">The step text.</param>
        /// <param name="body">The action that will perform the step.</param>
        public static void x(this string text, Action body) {
            LightBddContext.CurrentScenario.AddStep(text, body);
        }

        /// <summary>
        /// Defines a step in the current scenario.
        /// </summary>
        /// <param name="text">The step text.</param>
        /// <param name="body">The action that will perform the step.</param>
        public static void x(this string text, Action<IStepContext> body) {
            LightBddContext.CurrentScenario.AddStep(text, () => body(new StepContext(LightBddContext.CurrentScenario)));
        }

        /// <summary>
        /// Defines a step in the current scenario.
        /// </summary>
        /// <param name="text">The step text.</param>
        /// <param name="body">The action that will perform the step.</param>
        public static void x(this string text, Func<Task> body) {
            LightBddContext.CurrentScenario.AddAsyncStep(text, body);
        }

        /// <summary>
        /// Defines a step in the current scenario.
        /// </summary>
        /// <param name="text">The step text.</param>
        /// <param name="body">The action that will perform the step.</param>
        public static void x(this string text, Func<IStepContext, Task> body) {
            LightBddContext.CurrentScenario.AddAsyncStep(text, () => body(new StepContext(LightBddContext.CurrentScenario)));
        }

        private class StepContext : IStepContext {
            private readonly IScenarioContext _scenario;

            public StepContext(IScenarioContext scenario)
                => _scenario = scenario;

            public IStepContext Using(IDisposable disposable) {
                _scenario.Use(disposable);
                return this;
            }
        }
    }

    public interface IStepContext {
        /// <summary>
        /// Immediately registers the <see cref="IDisposable"/> object for disposal
        /// after all steps in the current scenario have been executed.
        /// </summary>
        /// <param name="disposable">The object to be disposed.</param>
        /// <returns>The current <see cref="IStepContext"/>.</returns>
        IStepContext Using(IDisposable disposable);
    }

    /// <summary>
    /// <see cref="IDisposable"/> extensions.
    /// </summary>
    public static class DisposableExtensions {
        /// <summary>
        /// Immediately registers the <see cref="IDisposable"/> object for disposal
        /// after all steps in the current scenario have been executed.
        /// </summary>
        /// <param name="disposable">The object to be disposed.</param>
        /// <param name="stepContext">The execution context for the current step.</param>
        public static T Using<T>(this T disposable, IStepContext stepContext) where T : IDisposable {
            stepContext.Using(disposable);
            return disposable;
        }
    }
}
