using System.Text.Json.Nodes;
using MSTest.Platform.Fixed.Warehouse;

namespace MSTest.Platform.Fixed.Reports;

/// <summary>
/// What the reports make of the files that <see cref="DeliveryNoteTests"/> and <see cref="LabelPrinterTests"/>
/// attach with <c>TestContext.AddResultFile</c>. The TRX report copies each attachment into its results
/// directory, and when a copy fails it says so on the output of the run with the path and the reason, and the
/// run goes on. The CTRF report lists the files of a test under <c>attachments</c> with their path, and the
/// files of an earlier <c>[Retry]</c> attempt under the <c>attachments</c> of that attempt in
/// <c>retryAttempts</c>. The CTRF option needs <c>EnableMicrosoftTestingExtensionsCtrfReport</c> in the
/// project file.
/// </summary>
[TestClass]
public sealed class AttachmentRunTests
{
    private const string Trx = "deliveries.trx";
    private const string Ctrf = "deliveries.ctrf.json";

    private static TestApplicationRun _run = null!;

    [ClassInitialize]
    public static async Task ReportTheDeliveryNotes(TestContext testContext) =>
        _run = await TestApplicationRun.StartAsync(
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(DeliveryNoteTests).FullName}|ClassName={typeof(LabelPrinterTests).FullName}",
            "--report-trx", "--report-trx-filename", Trx,
            "--report-ctrf", "--report-ctrf-filename", Ctrf);

    [ClassCleanup]
    public static void DeleteTheResults() => _run.Dispose();

    [TestMethod]
    public void Output_names_the_attachment_that_the_TRX_report_skips() =>
        Assert.Contains(
            $"Unable to copy attachment '{DeliveryNoteTests.UnprintedNote}' into the TRX results directory. The attachment will be skipped. Reason: FileNotFoundException.",
            _run.Output);

    [TestMethod]
    public void CTRF_report_lists_the_attachment_of_the_test()
    {
        var tests = JsonNode.Parse(File.ReadAllText(_run.PathOf(Ctrf)))!["results"]!["tests"]!.AsArray();
        var test = tests.Single(static test => (string?)test!["name"] == nameof(DeliveryNoteTests.Attaches_the_signed_delivery_note))!;

        var attachment = Assert.ContainsSingle(test["attachments"]!.AsArray());

        Assert.EndsWith(DeliveryNoteTests.SignedNote, (string?)attachment!["path"]);
    }

    [TestMethod]
    public void CTRF_report_lists_the_attachment_of_each_retry_attempt()
    {
        var tests = JsonNode.Parse(File.ReadAllText(_run.PathOf(Ctrf)))!["results"]!["tests"]!.AsArray();
        var test = tests.Single(static test => (string?)test!["name"] == nameof(LabelPrinterTests.Prints_the_label_at_the_second_attempt))!;
        var firstAttempt = Assert.ContainsSingle(test["retryAttempts"]!.AsArray())!;

        Assert.EndsWith("label-2.txt", (string?)Assert.ContainsSingle(test["attachments"]!.AsArray())!["path"]);
        Assert.AreEqual("failed", (string?)firstAttempt["status"]);
        Assert.EndsWith("label-1.txt", (string?)Assert.ContainsSingle(firstAttempt["attachments"]!.AsArray())!["path"]);
    }

    [TestMethod]
    public void Run_passes() => Assert.AreEqual(0, _run.ExitCode);
}
