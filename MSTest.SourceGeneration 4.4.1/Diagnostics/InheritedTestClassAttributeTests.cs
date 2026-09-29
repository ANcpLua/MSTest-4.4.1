using MSTest.SourceGenerated;

namespace MSTest.SourceGeneration.Diagnostics;

/// <summary>
/// MSTEST0069 reports a class that derives from a class with <c>[TestClass]</c> and does not declare the
/// attribute itself. <c>[TestClass]</c> is not inherited, so such a class is no test class, whether MSTest
/// discovers tests through the generated registry or through reflection: the registry has no entry for it,
/// and none of the tests it inherits run. MSTEST0030 reports the same class because it contains test methods.
/// The derived class declares <c>[TestClass]</c> itself.
/// </summary>
[TestClass]
public sealed class InheritedTestClassAttributeTests
{
    private static IEnumerable<Type> Classes => MSTestReflectionMetadata.TestClasses.Select(static entry => entry.Type);

    [TestMethod]
    public void Registry_has_no_entry_for_the_class_without_the_attribute()
        => Assert.DoesNotContain(typeof(ReportedTabSeparatedTests), Classes);

    [TestMethod]
    public void Registry_has_an_entry_for_the_class_with_the_attribute()
        => Assert.Contains(typeof(SemicolonSeparatedTests), Classes);
}

[TestClass]
public class CommaSeparatedTests
{
    protected virtual char Separator => ',';

    [TestMethod]
    public void Splits_the_line_at_the_separator() => Assert.HasCount(2, $"left{Separator}right".Split(Separator));
}

#pragma warning disable MSTEST0069, MSTEST0030
public sealed class ReportedTabSeparatedTests : CommaSeparatedTests
{
    protected override char Separator => throw new InvalidOperationException("The class is no test class, so its tests never run.");
}
#pragma warning restore MSTEST0069, MSTEST0030

[TestClass]
public sealed class SemicolonSeparatedTests : CommaSeparatedTests
{
    protected override char Separator => ';';
}
