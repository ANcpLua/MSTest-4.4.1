using MSTest.SourceGenerated;

namespace MSTest.SourceGeneration.Diagnostics;

/// <summary>
///     AOTSG0005 reports a parameter with <c>out</c>, <c>ref</c> or <c>in</c> on a method or constructor of a
///     test class that the generated registry can access, and the generator leaves the member out. A method
///     that returns its result instead has an entry. A private method is neither reported nor entered.
/// </summary>
[TestClass]
public sealed class ByRefParameterTests
{
    private static IEnumerable<string> Methods
        => MSTestReflectionMetadata.TestClasses.Single(static entry => entry.Type == typeof(ByRefParameterTests))
            .Methods.Select(static method => method.Name);

    [TestMethod]
    public void Registry_leaves_out_the_method_with_an_out_parameter()
    {
        Assert.IsTrue(TryParse("7", out var number));
        Assert.AreEqual(7, number);
        Assert.DoesNotContain(nameof(TryParse), Methods);
    }

    [TestMethod]
    public void Registry_leaves_out_the_method_with_a_ref_parameter()
    {
        var number = 7;
        Increment(ref number);

        Assert.AreEqual(8, number);
        Assert.DoesNotContain(nameof(Increment), Methods);
    }

    [TestMethod]
    public void Registry_leaves_out_the_method_with_an_in_parameter()
    {
        Assert.AreEqual(-7, Negate(7));
        Assert.DoesNotContain(nameof(Negate), Methods);
    }

    [TestMethod]
    public void Registry_has_an_entry_for_the_method_that_returns_its_result()
    {
        Assert.AreEqual(7, Parse("7"));
        Assert.Contains(nameof(Parse), Methods);
    }

    [TestMethod]
    public void Registry_has_no_entry_for_the_private_method()
    {
        Assert.IsTrue(TryParsePrivately("7", out var number));
        Assert.AreEqual(7, number);
        Assert.DoesNotContain(nameof(TryParsePrivately), Methods);
    }

    // AOTSG0005
    public static bool TryParse(string text, out int number)
    {
        return int.TryParse(text, out number);
    }

    // AOTSG0005
    public static void Increment(ref int number)
    {
        number++;
    }

    // AOTSG0005
    public static int Negate(in int number)
    {
        return -number;
    }

    public static int? Parse(string text)
    {
        return int.TryParse(text, out var number) ? number : null;
    }

    private static bool TryParsePrivately(string text, out int number)
    {
        return int.TryParse(text, out number);
    }
}