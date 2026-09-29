using System.Runtime.CompilerServices;
using MSTest.SourceGenerated;
using MSTest.SourceGeneration.Library.Lifecycle;

namespace MSTest.NativeAot.DynamicData;

/// <summary>
/// A source that the generated registry cannot access, a private source of the test class or a protected
/// source of a base class of another assembly, has no delegate, and MSTest reads it through reflection. The
/// private source stays readable in a trimmed executable because the generated registry roots the
/// properties, methods and fields of the test class, including the ones that are not public.
/// </summary>
[TestClass]
public sealed class ReflectedSourceTests : LifecycleTests
{
    private static string _sourceCall = string.Empty;

    private static IEnumerable<object[]> Cubes
    {
        get
        {
            _sourceCall = Environment.StackTrace;
            return [[2, 8]];
        }
    }

    [TestMethod]
    [DynamicData(nameof(Cubes))]
    public void Private_source_is_read_through_reflection(int number, int cube)
    {
        Assert.AreEqual(cube, number * number * number);
        Assert.IsEmpty(GeneratedSources());
        Assert.Contains("System.Reflection", _sourceCall);
    }

    [TestMethod]
    [DynamicData(nameof(Squares))]
    public void Protected_source_of_the_base_class_is_read_through_reflection(int number, int square)
    {
        Assert.AreEqual(square, number * number);
        Assert.IsEmpty(GeneratedSources());
    }

    private static IEnumerable<string> GeneratedSources([CallerMemberName] string test = "")
        => MSTestReflectionMetadata.TestClasses.Single(static entry => entry.Type == typeof(ReflectedSourceTests))
            .Methods.Single(method => method.Name == test)
            .DynamicDataSources.Select(static source => source.SourceName);
}
