using FluentAssertions;
using NSubstitute;

namespace Inspiring.BDD.Tests {
    public class ScenarioStepFactoryTests : FeatureBase {
        public static IEnumerable<object[]> AllStepTypes => Enum
            .GetValues<StepType>()
            .Select(x => new object[] { x });

        public static IEnumerable<object[]> AsyncSteps => Enum
            .GetValues<StepType>()
            .Where(s => s.ToString().StartsWith("Task"))
            .Select(x => new object[] { x });

        public static IEnumerable<object[]> SyncSteps => Enum
            .GetValues<StepType>()
            .Where(s => !s.ToString().StartsWith("Task"))
            .Select(x => new object[] { x });


        [Scenario]
        [MemberData(nameof(AllStepTypes))]
        protected void AddingStepsDoesNotInvokeThem(StepType type) {
            ScenarioStepFactory f = default!;
            bool wasCalled = false;

            GIVEN["a step factory with mocked context"] = () => f = new ScenarioStepFactory(Substitute.For<IScenarioContext>());

            WHEN["assigning a step delegate"] = () => f["<text>"] = () => CreateAsyncStep(type, () => {
                wasCalled = true;
                return Task.CompletedTask;
            });
            THEN["the step delegate is not called"] = () => wasCalled.Should().BeFalse();
        }

        [Scenario]
        [MemberData(nameof(SyncSteps))]
        protected void SynchronousStepExecution(StepType type) {
            IScenarioContext c = default!;
            ScenarioStepFactory f = default!;
            Action runTests = default!;
            int invocations = 0;

            GIVEN["a step factory"] = () => {
                c = Substitute.For<IScenarioContext>();
                c.AddStep(
                    Arg.Any<string>(),
                    Arg.Do<Action>(step => runTests = step));
                f = new ScenarioStepFactory(c, "PREFIX ");
            };
            WHEN["assigning a step function"] = () => f["<text>"] = CreateStep(type, () => invocations++);
            THEN["a step is added"] = () => c.Received().AddStep("PREFIX <text>", Arg.Any<Action>());
            WHEN["the test is run"] = () => runTests();
            THEN["the step is invoked"] = () => invocations.Should().Be(1);
        }

        [Scenario]
        [MemberData(nameof(AsyncSteps))]
        protected void AsyncStepExecution(StepType type) {
            IScenarioContext c = default!;
            ScenarioStepFactory f = default!;
            Func<Task> runTests = default!;
            int invocations = 0;

            GIVEN["a step factory"] = () => {
                c = Substitute.For<IScenarioContext>();
                c.AddAsyncStep(
                    Arg.Any<string>(),
                    Arg.Do<Func<Task>>(step => runTests = step));

                f = new ScenarioStepFactory(c, "PREFIX ");
            };
            WHEN["assigning a step function"] = () => f["<text>"] = CreateAsyncStep(type, () => {
                invocations++;
                return Task.CompletedTask;
            });
            THEN["a step is added"] = () => c.Received().AddAsyncStep("PREFIX <text>", Arg.Any<Func<Task>>());
            WHEN["the test is run"] = () => runTests();
            THEN["the step is invoked"] = () => invocations.Should().Be(1);

            GIVEN["an assigned step function"] = () => f["<text>"] = CreateAsyncStep(
                type,
                () => Task.FromException(new InvalidOperationException()));
            THEN["it is awaited when it is invoked"] = () =>
                runTests.Should().ThrowAsync<InvalidOperationException>(because: "otherwise the step has not been awaited");
        }

        [Scenario]
        protected void StepsReturningDispables(IScenarioContext c, ScenarioStepFactory f, IDisposable disposable) {
            GIVEN["a step factory"] = () => {
                c = Substitute.For<IScenarioContext>();

                c.AddAsyncStep(
                    Arg.Any<string>(),
                    Arg.Do<Func<Task>>(step => step().Wait()));

                c.AddStep(Arg.Any<string>(), Arg.Invoke());

                f = new ScenarioStepFactory(c);
            };

            GIVEN["a step that returns a disposable object"] = () => f[""] = CreateStep(
                StepType.Object,
                @return: disposable = Substitute.For<IDisposable>());
            THEN["the object is added to the scenario context"] = () => c.Received().Use(disposable);

            GIVEN["an async step that returns a disposable object"] = () => f[""] = CreateAsyncStep(
                StepType.TaskOfObject,
                @return: disposable = Substitute.For<IDisposable>());

            THEN["the object is added to the scenario context"] = () => c.Received().Use(disposable);
        }

        private static Delegate CreateStep(StepType type, Action? action = null, object? @return = null) {
            action ??= delegate { };

            return type switch {
                StepType.Void => action,
                StepType.Object => new Func<object?>(() => {
                    action();
                    return @return;
                }),
                StepType.Disposable => CreateStep(
                    StepType.Object,
                    action,
                    @return: Substitute.For<IDisposable>()),
                StepType.ValueType => new Func<int>(() => {
                    action();
                    return 0;
                }),
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }

        private static Delegate CreateAsyncStep(
            StepType type,
            Func<Task>? action = null,
            object? @return = null
        ) {
            action ??= () => Task.CompletedTask;

            return type switch {
                StepType.Task => action,
                StepType.TaskOfObject =>
                    new Func<Task<object?>>(async () => {
                        await action();
                        return @return;
                    }),
                StepType.TaskOfDisposable => CreateAsyncStep(
                    StepType.TaskOfObject,
                    action,
                    @return: Substitute.For<IDisposable>()),
                StepType.TaskOfValueType =>
                    new Func<Task<int>>(async () => {
                        await action();
                        return 0;
                    }),
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }

        public enum StepType {
            Void,
            Object,
            Disposable,
            ValueType,
            Task,
            TaskOfObject,
            TaskOfDisposable,
            TaskOfValueType
        }
    }
}
