using FluentAssertions;
using Inspiring.BDD.Core;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using NSubstitute;
using NSubstitute.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Inspiring.BDD.Tests {
    public class LightBddContextTests : FeatureBase {
        [Scenario]
        protected void Disposal(
            IScenarioRunner<NoContext> runner,
            LightBddContext context,
            List<IDisposable> disposables
        ) {
            GIVEN["a context"] = () => context = new LightBddContext(new object(), runner = Substitute.For<IScenarioRunner<NoContext>>());
            WHEN["using a some disposable objects"] = () => (
                disposables = new() {
                    Substitute.For<IDisposable>(),
                    Substitute.For<IDisposable>()
                }).ForEach(d => context.Use(d));
            AND["disposing the context"] = () => context.Dispose();
            THEN["all objects are disposed"] = () => disposables.ForEach(d => d.Received().Dispose());
        }

        [Scenario]
        protected void Backgrounds(TestFixture t) {
            GIVEN["a text fixture"] = () => t = Substitute.For<TestFixture>();
            WHEN["running the tests"] = () => RunScenario(t, "Test");
            THEN["the background is executed before the test method"] = () => Received.InOrder(() => {
                t.OnBackground();
                t.Test();
            });

        }

        private static Task RunScenario(object fixture, string testMethodName) {
            LightBddContext context = new (fixture, Substitute.For<IScenarioRunner<NoContext>>());

            using (ExecutionContext.SuppressFlow()) {
                return Task.Run(async () => {
                    MethodInfo testMethod = fixture
                        .GetType()
                        .GetMethod(testMethodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                            ?? throw new ArgumentException($"Method '{testMethodName}' not found on fixture.");

                    LightBddContext.SetScenario(testMethod);

                    try {
                        await context.Run();
                    } finally {
                        LightBddContext.ClearScenario();
                    }
                });
            }
        }


        public abstract class TestFixture {
            [Background]
            protected void Background() => OnBackground();

            public abstract void OnBackground();

            [Scenario]
            public abstract void Test();

            protected abstract Task Run();
        }
    }
}
