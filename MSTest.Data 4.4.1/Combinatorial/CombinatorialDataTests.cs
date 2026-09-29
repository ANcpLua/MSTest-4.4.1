using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting.Combinatorial;

namespace MSTest.Data.Combinatorial;

/// <summary>
/// <see cref="CombinatorialDataAttribute"/> runs a test once for every combination of the values of its
/// parameters. A parameter takes its values from its type or from the provider attribute it carries. The rows
/// advance like an odometer, the last parameter first, and each row is named after its arguments.
/// </summary>
[TestClass]
public sealed class CombinatorialDataTests(TestContext testContext)
{
    [TestMethod]
    [CombinatorialData]
    public void Runs_once_for_every_combination(
        bool express,
        [CombinatorialValues("DE", "AT")] string country,
        [CombinatorialRange(1, 3)] int parcels)
        => Assert.AreEqual(
            $"""{nameof(Runs_once_for_every_combination)} ({express},"{country}",{parcels})""",
            testContext.TestDisplayName);

    [TestMethod]
    public void The_rows_are_the_whole_product_in_odometer_order()
    {
        MethodInfo test = typeof(CombinatorialDataTests).GetMethod(nameof(Runs_once_for_every_combination))!;

        IEnumerable<(bool, string, int)> rows = new CombinatorialDataAttribute()
            .GetData(test)
            .Select(row => ((bool)row[0]!, (string)row[1]!, (int)row[2]!));

        Assert.AreSequenceEqual(
            [
                (true, "DE", 1), (true, "DE", 2), (true, "DE", 3),
                (true, "AT", 1), (true, "AT", 2), (true, "AT", 3),
                (false, "DE", 1), (false, "DE", 2), (false, "DE", 3),
                (false, "AT", 1), (false, "AT", 2), (false, "AT", 3),
            ],
            rows);
    }
}
