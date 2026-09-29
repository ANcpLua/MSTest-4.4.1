using MSTest.SourceGenerated;

namespace MSTest.SourceGeneration.Diagnostics;

/// <summary>
/// AOTSG0002 reports a class with <c>[TestClass]</c> that is generic or nested in a generic class: the
/// generator writes no entry for a type whose type parameters are open. The entry belongs to the closed
/// class that derives from the generic one, and the registry calls the tests that this class inherits. A
/// generic base class goes without <c>[TestClass]</c>.
/// </summary>
[TestClass]
public sealed class GenericTestClassTests
{
    private static IEnumerable<Type> Classes => MSTestReflectionMetadata.TestClasses.Select(static entry => entry.Type);

    [TestMethod]
    public void Registry_has_an_entry_for_each_closed_class()
    {
        Assert.Contains(typeof(ReportedInt32ParsingTests), Classes);
        Assert.Contains(typeof(Int32ParsingTests), Classes);
    }

    [TestMethod]
    public void Registry_has_no_entry_for_a_generic_class()
    {
        Assert.DoesNotContain(typeof(ReportedParsingTests<int>), Classes);
        Assert.DoesNotContain(typeof(ParsingTests<int>), Classes);
    }

    [TestMethod]
    public void Registry_has_no_entry_for_the_class_in_a_generic_class()
        => Assert.DoesNotContain(typeof(ParsingTests<int>.ReportedNestedTests), Classes);
}

// AOTSG0002
[TestClass]
public abstract class ReportedParsingTests<T>
    where T : IParsable<T>
{
    [TestMethod]
    public void Inherited_test_is_called_by_the_registry()
    {
        Assert.AreEqual(T.Parse("7", provider: null), T.Parse("07", provider: null));
        Assert.Contains("MSTest.SourceGenerated.MSTestReflectionMetadata", Environment.StackTrace);
    }
}

[TestClass]
public sealed class ReportedInt32ParsingTests : ReportedParsingTests<int>;

public abstract class ParsingTests<T>
    where T : IParsable<T>
{
    [TestMethod]
    public void Inherited_test_is_called_by_the_registry()
    {
        Assert.AreEqual(T.Parse("7", provider: null), T.Parse("07", provider: null));
        Assert.Contains("MSTest.SourceGenerated.MSTestReflectionMetadata", Environment.StackTrace);
    }

    // AOTSG0002
    [TestClass]
    public sealed class ReportedNestedTests
    {
        [TestMethod]
        public void Never_runs() => Assert.Fail();
    }
}

[TestClass]
public sealed class Int32ParsingTests : ParsingTests<int>;
