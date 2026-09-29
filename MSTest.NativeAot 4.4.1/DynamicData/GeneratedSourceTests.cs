using System.Reflection;
using System.Runtime.CompilerServices;
using MSTest.SourceGenerated;

namespace MSTest.NativeAot.DynamicData;

/// <summary>
/// <c>[DynamicData]</c> in a trimmed executable. For a source that is a public or internal static property,
/// method without parameters or field, the generator writes a delegate that reads the member directly, and
/// MSTest takes the rows from that delegate. The same holds for a source of another class and for the method
/// behind <c>DynamicDataDisplayName</c>.
/// </summary>
[TestClass]
public sealed class GeneratedSourceTests(TestContext testContext)
{
    private const string Registry = "MSTest.SourceGenerated.MSTestReflectionMetadata";

    public static readonly IEnumerable<object[]> Field = [[3, 9]];

    private static string _propertyCall = string.Empty;
    private static string _methodCall = string.Empty;
    private static string _displayNameCall = string.Empty;

    public static IEnumerable<object[]> Property
    {
        get
        {
            _propertyCall = Environment.StackTrace;
            return [[1, 1]];
        }
    }

    public static IEnumerable<object[]> Method()
    {
        _methodCall = Environment.StackTrace;
        return [[2, 4]];
    }

    public static string Name(MethodInfo method, object[] row)
    {
        _displayNameCall = Environment.StackTrace;
        return $"{row[0]} costs {row[1]}";
    }

    [TestMethod]
    [DynamicData(nameof(Property))]
    public void Property_is_read_by_the_registry(int number, int square)
    {
        Assert.AreEqual(square, number * number);
        Assert.Contains(Registry, _propertyCall);
        Assert.DoesNotContain("System.Reflection", _propertyCall);
    }

    [TestMethod]
    [DynamicData(nameof(Method))]
    public void Method_is_called_by_the_registry(int number, int square)
    {
        Assert.AreEqual(square, number * number);
        Assert.Contains(Registry, _methodCall);
        Assert.DoesNotContain("System.Reflection", _methodCall);
    }

    [TestMethod]
    [DynamicData(nameof(Field))]
    public void Field_has_a_delegate_in_the_registry(int number, int square)
    {
        Assert.AreEqual(square, number * number);
        Assert.AreSequenceEqual([nameof(Field)], GeneratedSources());
    }

    [TestMethod]
    [DynamicData(nameof(Catalog.Prices), typeof(Catalog))]
    public void Source_of_another_class_has_a_delegate_in_the_registry(string product, int price)
    {
        Assert.IsGreaterThan(0, price);
        Assert.IsNotEmpty(product);
        Assert.AreSequenceEqual([nameof(Catalog.Prices)], GeneratedSources());
    }

    [TestMethod]
    [DynamicData(nameof(Catalog.Prices), typeof(Catalog), DynamicDataDisplayName = nameof(Name))]
    public void Display_name_is_written_by_the_registry(string product, int price)
    {
        Assert.AreEqual($"{product} costs {price}", testContext.TestDisplayName);
        Assert.Contains(Registry, _displayNameCall);
        Assert.DoesNotContain("System.Reflection", _displayNameCall);
    }

    private static IEnumerable<string> GeneratedSources([CallerMemberName] string test = "")
        => MSTestReflectionMetadata.TestClasses.Single(static entry => entry.Type == typeof(GeneratedSourceTests))
            .Methods.Single(method => method.Name == test)
            .DynamicDataSources.Select(static source => source.SourceName);
}
