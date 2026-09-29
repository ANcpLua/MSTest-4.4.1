using System.Xml.Linq;
using MSTest.Platform.Fixed.Warehouse;

namespace MSTest.Platform.Fixed.Reports;

/// <summary>
/// The JUnit report writes the body of a <c>failure</c> element as Java prints an exception: the <c>type</c>
/// attribute, a colon, the <c>message</c> attribute, and the stack trace below. A CI system that shows the body and
/// not the <c>message</c> attribute shows the message too. For a test that throws, MSTest reports its own exception
/// type, <c>MSTestTestNodeException</c>, and a message whose first line names the test and whose second line holds
/// the type and the message of the thrown exception. Here a run reports
/// <see cref="DamagedParcelTests.Rejects_the_damaged_parcel"/> with <c>--report-junit</c>, which needs
/// <c>EnableMicrosoftTestingExtensionsJUnitReport</c> in the project file.
/// </summary>
[TestClass]
public sealed class JUnitFailureBodyRunTests
{
    private const string JUnit = "parcel.xml";

    private static TestApplicationRun _run = null!;
    private static XElement _failure = null!;

    [ClassInitialize]
    public static async Task ReportTheDamagedParcel(TestContext testContext)
    {
        _run = await TestApplicationRun.StartAsync(
            testContext.CancellationToken,
            "--filter", $"FullyQualifiedName={typeof(DamagedParcelTests).FullName}.{nameof(DamagedParcelTests.Rejects_the_damaged_parcel)}",
            "--report-junit", "--report-junit-filename", JUnit);
        _failure = XDocument.Load(_run.PathOf(JUnit)).Descendants("failure").Single();
    }

    [ClassCleanup]
    public static void DeleteTheResults() => _run.Dispose();

    [TestMethod]
    public void Body_starts_with_the_type_and_the_message_attribute()
    {
        var header = $"{_failure.Attribute("type")!.Value}: {_failure.Attribute("message")!.Value}\n";

        Assert.StartsWith(header.ReplaceLineEndings("\n"), _failure.Value.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public void Type_is_the_exception_type_that_MSTest_reports() =>
        Assert.AreEqual("Microsoft.VisualStudio.TestTools.UnitTesting.MSTestTestNodeException", _failure.Attribute("type")!.Value);

    [TestMethod]
    public void Second_line_holds_the_type_and_the_message_of_the_thrown_exception() =>
        Assert.StartsWith(
            $"{typeof(InvalidOperationException).FullName}: {DamagedParcelTests.Damaged} ---> {typeof(FormatException).FullName}: ",
            _failure.Value.ReplaceLineEndings("\n").Split('\n')[1]);

    [TestMethod]
    public void Body_ends_with_the_stack_trace() =>
        Assert.Contains($"at {typeof(DamagedParcelTests).FullName}.{nameof(DamagedParcelTests.Rejects_the_damaged_parcel)}()", _failure.Value);

    [TestMethod]
    public void Run_fails() => Assert.AreEqual(2, _run.ExitCode);
}
