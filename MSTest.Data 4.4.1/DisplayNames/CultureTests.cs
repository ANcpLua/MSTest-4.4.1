using System.Globalization;
using System.Reflection;

namespace MSTest.Data.DisplayNames;

/// <summary>
///     The arguments in the name of a data row are written in the current culture. The name of a row with a
///     fraction therefore depends on the culture of the process that runs the test.
/// </summary>
[TestClass]
public sealed class CultureTests(TestContext testContext)
{
    [TestMethod]
    [DataRow(1.5)]
    public void Weighs(double kilograms)
    {
        Assert.AreEqual(
            $"{nameof(Weighs)} ({kilograms.ToString(CultureInfo.CurrentCulture)})",
            testContext.TestDisplayName);
    }

    [TestMethod]
    [DataRow("en-US", "Weighs (1.5)")]
    [DataRow("de-DE", "Weighs (1,5)")]
    public void A_fraction_is_written_as_the_culture_writes_it(string culture, string expected)
    {
        var test = typeof(CultureTests).GetMethod(nameof(Weighs))!;
        var row = test.GetCustomAttribute<DataRowAttribute>()!;
        var current = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);

            Assert.AreEqual(expected, row.GetDisplayName(test, row.Data));
        }
        finally
        {
            CultureInfo.CurrentCulture = current;
        }
    }
}