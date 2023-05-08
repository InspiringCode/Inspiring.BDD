namespace Inspiring.BDD.Examples {
    public class CalculatorFeature : FeatureBase {
        public static IEnumerable<object[]> CalculationExamples = new[] {
            new object[] { "1", new CalculatorCase(1, 3, 4) },
            new object[] { "2", new CalculatorCase(1, -3, -2) },
            new object[] { "3", new CalculatorCase(2, 3, 5) }
        };

        public static IEnumerable<object[]> CalculationExamples2 = new[] {
            new object[] { 1, 3, 4 },
            new object[] { 1, -3, -2 },
            new object[] { 2, 3, 5 }
        };

        //[Scenario]
        public void AddingTwoNumbers(Calculator c, int result, int x, int y) {
            GIVEN["a calculator"] = () => c = new Calculator();
            WHEN["adding two numbers"] = () => result = c.Add(5, 3);
            THEN["the result is correct"] = () => Assert.Equal(8, result);
            WHEN["the two operands are too large"] = () => (x, y) = (int.MaxValue, 1);
            THEN["an exception is thrown"] = () => Assert.Throws<OverflowException>(() => c.Add(int.MaxValue, 1));
        }

        [Scenario]
        [MemberData(nameof(CalculationExamples))]
        public void ExampleForParametricTests(string s, CalculatorCase c) {
            Calculator calc = default!;
            int result = 0;

            calc = new Calculator();
            result = calc.Add(c.X, c.Y);
            Assert.Equal(c.Result, result);

            //GIVEN["a calculator"] = () => calc = new Calculator();
            //WHEN["adding"] = () => result = calc.Add(c.X, c.Y);
            //THEN["the result is correct"] = () => Assert.Equal(c.Result, result);
        }
        //[Scenario]
        [MemberData(nameof(CalculationExamples2))]
        public void ExampleForParametricTests2(int x, int y, int expected) {
            Calculator calc = default!;
            int result = 0;

            //calc = new Calculator();
            //result = calc.Add(x, y);
            //Assert.Equal(expected, result);

            GIVEN["a calculator"] = () => calc = new Calculator();
            WHEN["adding"] = () => result = calc.Add(x, y);
            THEN["the result is correct"] = () => Assert.Equal(expected, result);
        }
    }

    public class Calculator {
        public int Add(int x, int y)
            => checked(x + y);
    }

    public record CalculatorCase(int X, int Y, int Result);
}