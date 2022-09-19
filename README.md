# Inspiring.BDD: The fun way to write tests

Inspiring.BDD is a lightweight extension of [LightBDD](https://github.com/LightBDD/LightBDD/) that allows to write tests in a concise and readable manner and that provides a small adapter to run tests written for [xBehave.net](https://github.com/adamralph/xbehave.net) on LightBDD without much change to the tests.

# Example

```csharp
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
```

Test output:
```
SCENARIO: Adding Two Numbers
  STEP 1/5: GIVEN a calculator...
  STEP 1/5: GIVEN a calculator (Passed after 6ms)
  STEP 2/5: WHEN adding two numbers...
  STEP 2/5: WHEN adding two numbers (Passed after <1ms)
  STEP 3/5: THEN the result is correct...
  STEP 3/5: THEN the result is correct (Passed after 1ms)
  STEP 4/5: WHEN the tow operands are too large...
  STEP 4/5: WHEN the tow operands are too large (Passed after <1ms)
  STEP 5/5: THEN an exception is thrown...
  STEP 5/5: THEN an exception is thrown (Passed after 1ms)
```

Things to note:
* The method parameters serve as local test variables and null or the default is passed by Inspiring.BDD.
* The syntax is inspired by [xBehave.net](https://github.com/adamralph/xbehave.net)

# More examples

More examples can be found in the Inspiring.BDD.Tests project in the source code.

# xBehave.​net compatibility mode

You can continue to run your xBehave.net](https://github.com/adamralph/xbehave.net) tests without referencing xBehave itself by following the following steps:
1. Reference the Inspiring.BDD NuGet package.
1. Add the `LightBddScope` attribute to your test assembly.
1. Copy the `Inspiring.BDD.Examples\XBehaveAdapter.cs` file your test project.

Then most of your xBehave tests should run as with xBehave itself. Note that the adapter does not mimic all features of xBehave. If you are missing some features of xBehave you can easily adapt must features yourself (PRs welcome) or you can open an issue here on github.
