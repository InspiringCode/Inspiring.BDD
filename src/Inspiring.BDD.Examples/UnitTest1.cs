namespace Inspiring.BDD.Examples {
    public class CalculatorFeature : FeatureBase {
        [Scenario]
        public void AddingTwoNumbers(Calculator c, int result, int x, int y) {
            GIVEN["a calculator"] = () => c = new Calculator();
            WHEN["adding two numbers"] = () => result = c.Add(5, 3);
            THEN["the result is correct"] = () => Assert.Equal(8, result);
            WHEN["the two operands are too large"] = () => (x, y) = (int.MaxValue, 1);
            THEN["an exception is thrown"] = () => Assert.Throws<OverflowException>(() => c.Add(int.MaxValue, 1));
        }
    }

    public class Calculator {
        public int Add(int x, int y)
            => checked(x + y);
    }
}