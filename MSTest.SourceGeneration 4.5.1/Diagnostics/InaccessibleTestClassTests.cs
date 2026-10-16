using MSTest.SourceGenerated;

namespace MSTest.SourceGeneration.Diagnostics;

/// <summary>
/// AOTSG0003 reports a class with <c>[TestClass]</c> that the generated registry cannot reach: a file-local
/// class, or a class that is private, protected or private protected or is nested in such a class. The
/// generator writes no entry for it, so MSTest does not see the class and its tests never run. MSTEST0002
/// reports the same classes and MSTEST0003 their tests, because a test class has to be public.
/// </summary>
[TestClass]
public sealed class InaccessibleTestClassTests
{
    private static IEnumerable<Type> Classes => MSTestReflectionMetadata.TestClasses.Select(static entry => entry.Type);

    [TestMethod]
    public void Registry_has_no_entry_for_the_file_local_class() => Assert.DoesNotContain(typeof(FileLocalTests), Classes);

    [TestMethod]
    public void Registry_has_no_entry_for_the_private_class() => Assert.DoesNotContain(typeof(PrivateTests), Classes);

    [TestMethod]
    public void Registry_has_no_entry_for_the_class_in_a_private_class()
        => Assert.DoesNotContain(typeof(PrivateContainer.ContainedTests), Classes);

    [TestMethod]
    public void Registry_has_an_entry_for_the_public_nested_class() => Assert.Contains(typeof(PublicTests), Classes);

    // AOTSG0003
#pragma warning disable MSTEST0002, MSTEST0003
    [TestClass]
    private sealed class PrivateTests
    {
        [TestMethod]
        public void Never_runs() => Assert.Fail();
    }
#pragma warning restore MSTEST0002, MSTEST0003

    private static class PrivateContainer
    {
        // AOTSG0003
#pragma warning disable MSTEST0002, MSTEST0003
        [TestClass]
        public sealed class ContainedTests
        {
            [TestMethod]
            public void Never_runs() => Assert.Fail();
        }
#pragma warning restore MSTEST0002, MSTEST0003
    }

    [TestClass]
    public sealed class PublicTests
    {
        [TestMethod]
        public void Is_called_by_the_registry()
            => Assert.Contains("MSTest.SourceGenerated.MSTestReflectionMetadata", Environment.StackTrace);
    }
}

// AOTSG0003
#pragma warning disable MSTEST0002, MSTEST0003
[TestClass]
file sealed class FileLocalTests
{
    [TestMethod]
    public void Never_runs() => Assert.Fail();
}
#pragma warning restore MSTEST0002, MSTEST0003
