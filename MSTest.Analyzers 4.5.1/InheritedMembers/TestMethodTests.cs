using MSTest.Analyzers.CurrentBase;
using MSTest.Analyzers.LegacyBase;

namespace MSTest.Analyzers.InheritedMembers;

/// <summary>
/// MSTEST0082 reports a test class for each <c>[TestMethod]</c> and <c>[DataTestMethod]</c> method that it
/// inherits from a base class compiled against MSTest 3. The inherited tests are not discovered: the test of
/// the class itself is the only one that runs.
/// </summary>
[TestClass]
#pragma warning disable MSTEST0082
public sealed class LegacyTrimmingTests : LegacyNormalizerContract
#pragma warning restore MSTEST0082
{
    protected override string Normalize(string text) => text.Trim();

    [TestMethod]
    public void Removes_the_padding() => Assert.AreEqual("padded", Normalize(" padded "));

    [ClassCleanup]
    public static void Inherited_tests_were_not_run() => Assert.AreEqual(0, Runs);
}

/// <summary>
/// The base class is compiled against the MSTest version of the test project, and the inherited tests are
/// discovered and run as tests of this class next to its own test.
/// </summary>
[TestClass]
public sealed class TrimmingTests : NormalizerContract
{
    protected override string Normalize(string text) => text.Trim();

    [TestMethod]
    public void Removes_the_padding() => Assert.AreEqual("padded", Normalize(" padded "));

    [ClassCleanup]
    public static void Inherited_tests_were_run() => Assert.IsPositive(Runs);
}
