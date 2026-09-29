using MSTest.Showcase;

namespace MSTest.NativeAot.Diagnostics;

/// <summary>
/// A generic test in a Native AOT project, which AOTSG0004 reports. The generator writes no delegate for it,
/// so MSTest calls it through reflection. Under <c>dotnet run</c> both rows pass. In the native executable
/// the row with the string passes and the row with the integer fails with a
/// <see cref="NotSupportedException"/>, because the executable has no native code for the method with a
/// value type as its type argument.
/// </summary>
[TestClass]
public sealed class GenericTestMethodTests
{
    // AOTSG0004
    [TestMethod]
    [Explicit]
    [DataRow(1)]
    [DataRow("one")]
    public void Generic_test_is_called_through_reflection<T>(T value)
    {
        Assert.IsNotNull(value);
        Assert.Contains("System.Reflection", Environment.StackTrace);
    }
}
