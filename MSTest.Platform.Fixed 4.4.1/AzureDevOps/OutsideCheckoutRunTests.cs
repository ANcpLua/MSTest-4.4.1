using MSTest.Platform.Fixed.Warehouse;

namespace MSTest.Platform.Fixed.AzureDevOps;

/// <summary>
/// The Azure DevOps report annotates a failed test with the file and the line of the test, relative to the
/// root of the git checkout that holds the test application. A test application outside a git checkout, as a
/// published application or one in a container is, gets an annotation with the message only, and the run goes
/// on. Here the application runs once from its output folder in this repository and once from a copy of that
/// folder in the temporary folder, which no git checkout holds, and both runs report
/// <see cref="DamagedParcelTests.Rejects_the_damaged_parcel"/> with <c>--report-azdo</c> and
/// <c>TF_BUILD=true</c>.
/// </summary>
[TestClass]
public sealed class OutsideCheckoutRunTests
{
    private const string Issue = "##vso[task.logissue type=error";

    private static DirectoryInfo _copy = null!;
    private static TestApplicationRun _inCheckout = null!;
    private static TestApplicationRun _outsideCheckout = null!;

    [ClassInitialize]
    public static async Task RunInsideAndOutsideTheCheckout(TestContext testContext)
    {
        _copy = Directory.CreateTempSubdirectory();
        foreach (var file in Directory.EnumerateFiles(AppContext.BaseDirectory, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(_copy.FullName, Path.GetRelativePath(AppContext.BaseDirectory, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target);
        }

        _inCheckout = await ReportAsync(Environment.ProcessPath!, testContext);
        _outsideCheckout = await ReportAsync(Path.Combine(_copy.FullName, Path.GetFileName(Environment.ProcessPath)!), testContext);
    }

    [ClassCleanup]
    public static void DeleteTheCopy()
    {
        _inCheckout.Dispose();
        _outsideCheckout.Dispose();
        _copy.Delete(recursive: true);
    }

    [TestMethod]
    public void Annotation_in_the_checkout_names_the_file_of_the_test() =>
        Assert.Contains(
            $"{Issue};sourcepath=MSTest.Platform.Fixed 4.4.1/Warehouse/{nameof(DamagedParcelTests)}.cs;",
            _inCheckout.Output);

    [TestMethod]
    public void Annotation_outside_a_checkout_has_the_message_only()
    {
        var issue = Assert.ContainsSingle(_outsideCheckout.Output.Split(Environment.NewLine).Where(static line => line.StartsWith(Issue, StringComparison.Ordinal)));

        Assert.StartsWith($"{Issue}]", issue);
        Assert.Contains(DamagedParcelTests.Damaged, issue);
    }

    [TestMethod]
    public void Run_outside_a_checkout_reports_the_failure_and_ends_normally()
    {
        Assert.Contains("failed: 1", _outsideCheckout.Output);
        Assert.AreEqual(2, _outsideCheckout.ExitCode);
        Assert.AreEqual(string.Empty, _outsideCheckout.Error);
    }

    private static Task<TestApplicationRun> ReportAsync(string application, TestContext testContext) =>
        TestApplicationRun.StartAsync(
            application,
            Directory.CreateTempSubdirectory(),
            new Dictionary<string, string> { ["TF_BUILD"] = "true" },
            testContext.CancellationToken,
            "--filter", $"FullyQualifiedName={typeof(DamagedParcelTests).FullName}.{nameof(DamagedParcelTests.Rejects_the_damaged_parcel)}",
            "--report-azdo");
}
