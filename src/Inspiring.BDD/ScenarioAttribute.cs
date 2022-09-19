using LightBDD.XUnit2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Inspiring.BDD {
    /// <summary>
    /// Applied to a method to indicate the definition of a scenario. A scenario can also be fed 
    /// examples from a data source, mapping to parameters on the scenario method. If the data source 
    /// contains multiple rows, then the scenario method is executed multiple times (once with each 
    /// data row). Examples can be fed to the scenario by applying one or more instances of any 
    /// attribute inheriting from <see cref="Xunit.Sdk.DataAttribute"/> (e.g. <see cref="InlineDataAttribute"/>
    /// or <see cref="MemberDataAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    [XunitTestCaseDiscoverer("Inspiring.BDD.Core.ScenarioTestCaseDiscoverer", "Inspiring.BDD")]
    public class ScenarioAttribute : FactAttribute {
    }

    namespace Core {
        /// <summary>
        /// Basically a copy of the (internal) LightBDD ScenarioTestCaseDiscoverer returning a 
        /// reimplementation of the LightBDD ScenarioTestCase.
        /// </summary>
        internal class ScenarioTestCaseDiscoverer : TheoryDiscoverer {
            public ScenarioTestCaseDiscoverer(IMessageSink diagnosticMessageSink)
                : base(diagnosticMessageSink) { }

            public override IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute) {
                if (!testMethod.Method.GetCustomAttributes(typeof(DataAttribute)).Any()) {
                    return new[] {
                    new ScenarioTestCase(
                        DiagnosticMessageSink,
                        discoveryOptions.MethodDisplayOrDefault(),
                        discoveryOptions.MethodDisplayOptionsOrDefault(),
                        testMethod)
                };
                }

                return base.Discover(discoveryOptions, testMethod, theoryAttribute);
            }

            protected override IEnumerable<IXunitTestCase> CreateTestCasesForDataRow(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute, object[] dataRow) {
                yield return new ScenarioTestCase(
                    DiagnosticMessageSink,
                    discoveryOptions.MethodDisplayOrDefault(),
                    discoveryOptions.MethodDisplayOptionsOrDefault(),
                    testMethod,
                    dataRow);
            }

            protected override IEnumerable<IXunitTestCase> CreateTestCasesForTheory(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute) {
                Type multiTestCaseType = typeof(LightBddScopeAttribute)
                        .Assembly
                        .GetType("LightBDD.XUnit2.Implementation.Customization.ScenarioMultiTestCase")!;

                yield return (IXunitTestCase)Activator.CreateInstance(
                    multiTestCaseType,
                    DiagnosticMessageSink,
                    discoveryOptions.MethodDisplayOrDefault(),
                    discoveryOptions.MethodDisplayOptionsOrDefault(),
                    testMethod);
            }

            /// <summary>
            /// Reimplements the LightBDD ScenarioTestCase.
            /// </summary>
            private class ScenarioTestCase : XunitTestCase {
                [EditorBrowsable(EditorBrowsableState.Never)]
                [Obsolete("Called by the de-serializer", true)]
                public ScenarioTestCase() { }

                public ScenarioTestCase(
                    IMessageSink diagnosticMessageSink,
                    TestMethodDisplay defaultMethodDisplay,
                    TestMethodDisplayOptions defaultMethodDisplayOptions,
                    ITestMethod testMethod,
                    object[]? testMethodArguments = null
                ) : base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, testMethodArguments) { }

                /// <remarks>
                /// We use a trick here to run some code before and after the actual test method:
                ///   1) We remember the actual test method in an async local member of <see 
                ///      cref="LightBddContext"/>.
                ///   2) We use a <see cref="CustomMethodInfo"/> to trick XUnit to call the "Run"
                ///      method of the test class instance. Since we also support parameterized tests,
                ///      the test class has to provide overloads for any number of parameters.
                ///   3) The Run method executes some code before, then executes the method set in
                ///      step (1) and finally some code afterwards.
                /// </remarks>
                public override async Task<RunSummary> RunAsync(
                    IMessageSink diagnosticMessageSink,
                    IMessageBus messageBus,
                    object[] constructorArguments,
                    ExceptionAggregator aggregator,
                    CancellationTokenSource cancellationTokenSource
                ) {
                    LightBddContext.SetScenario(Method.ToRuntimeMethod());

                    try {
                        MethodInfo m = typeof(LightBddFeature)
                            .GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                            .SingleOrDefault(m => m.Name == "Run" && m.GetParameters().Length == (TestMethodArguments?.Length ?? 0))
                                ?? throw new InvalidOperationException("FeatureFixture.Run method not found.");

                        IMethodInfo info = new CustomMethodInfo(TestMethod.Method, m);
                        Method = info;
                        TestMethod = new TestMethod(TestMethod.TestClass, info);

                        Type runnerType = typeof(LightBddScopeAttribute)
                            .Assembly
                            .GetType("LightBDD.XUnit2.Implementation.Customization.ScenarioTestCaseRunner")!;

                        XunitTestCaseRunner runner = (XunitTestCaseRunner)Activator.CreateInstance(
                            runnerType,
                            this,
                            DisplayName,
                            SkipReason,
                            constructorArguments,
                            TestMethodArguments,
                            messageBus,
                            aggregator,
                            cancellationTokenSource
                        )!;

                        return await runner.RunAsync();
                    } finally {
                        LightBddContext.ClearScenario();
                    }
                }
            }

            /// <summary>
            /// Forwards all members to the given <see cref="IMethodInfo"/>, except <see 
            /// cref="IReflectionMethodInfo.MethodInfo"/>.
            /// </summary>
            private class CustomMethodInfo : IReflectionMethodInfo {
                private readonly IMethodInfo _inner;
                private readonly MethodInfo _methodInfo;

                public MethodInfo MethodInfo => _methodInfo;

                public bool IsAbstract => _inner.IsAbstract;

                public bool IsGenericMethodDefinition => _inner.IsGenericMethodDefinition;

                public bool IsPublic => _inner.IsPublic;

                public bool IsStatic => _inner.IsStatic;

                public string Name => _inner.Name;

                public ITypeInfo ReturnType => _inner.ReturnType;

                public ITypeInfo Type => _inner.Type;

                public CustomMethodInfo(IMethodInfo inner, MethodInfo methodInfo) {
                    _inner = inner;
                    _methodInfo = methodInfo;
                }

                public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
                    => _inner.GetCustomAttributes(assemblyQualifiedAttributeTypeName);

                public IEnumerable<ITypeInfo> GetGenericArguments()
                    => _inner.GetGenericArguments();

                public IEnumerable<IParameterInfo> GetParameters()
                    => _inner.GetParameters();

                public IMethodInfo MakeGenericMethod(params ITypeInfo[] typeArguments)
                    => _inner.MakeGenericMethod(typeArguments);
            }
        }
    }
}
