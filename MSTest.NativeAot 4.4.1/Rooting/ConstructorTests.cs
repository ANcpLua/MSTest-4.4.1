namespace MSTest.NativeAot.Rooting;

/// <summary>
/// A test class that takes the <see cref="TestContext"/> through its constructor. The generator keeps the
/// constructor in the trimmed executable and writes a delegate for it, so MSTest creates the class there as
/// it does under <c>dotnet run</c>.
/// </summary>
[TestClass]
public sealed class ConstructorTests(TestContext testContext)
{
    private readonly string _constructorCall = Environment.StackTrace;

    [TestMethod]
    public void Constructor_receives_the_test_context()
        => Assert.AreEqual(nameof(Constructor_receives_the_test_context), testContext.TestName);

    [TestMethod]
    public void Constructor_is_called_by_the_registry()
        => Assert.Contains("MSTest.SourceGenerated.MSTestReflectionMetadata", _constructorCall);
}
