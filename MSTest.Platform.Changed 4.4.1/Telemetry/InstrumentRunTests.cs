using OpenTelemetry.Metrics;

namespace MSTest.Platform.Changed.Telemetry;

/// <summary>
/// The instruments that the platform records for a run. <c>test.case.result.count</c> counts the finished
/// tests and <c>test.case.duration</c> measures them in seconds, both for each status and suite.
/// <c>test.case.active</c> counts the tests that are running, and <c>test.run.duration</c> measures the run
/// in seconds with its status and its exit code. A count has the unit <c>{test}</c>.
/// </summary>
[TestClass]
public sealed class InstrumentRunTests
{
    private static TestApplicationRun _run = null!;
    private static IReadOnlyList<Instrument> _instruments = null!;

    [ClassInitialize]
    public static async Task RunTheCheckoutAndTheDeclinedCard(TestContext testContext)
    {
        _run = await TestApplicationRun.StartAsync(
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(CheckoutTests).FullName}|ClassName={typeof(DeclinedCardTests).FullName}");
        _instruments = _run.ReadInstruments();
    }

    [ClassCleanup]
    public static void DeleteTheResults() => _run.Dispose();

    [TestMethod]
    [DataRow("test.case.result.count", "{test}", MetricType.LongSum)]
    [DataRow("test.case.duration", "s", MetricType.Histogram)]
    public void Instrument_of_the_tests_has_a_series_for_each_status_and_suite(string name, string unit, MetricType kind)
    {
        var series = _instruments.Where(instrument => instrument.Name == name).ToArray();

        Assert.AreSequenceEqual(
            [$"pass {nameof(CheckoutTests)}", $"fail {nameof(DeclinedCardTests)}", $"skipped {nameof(DeclinedCardTests)}"],
            series.Select(static instrument => $"{instrument.Attributes["test.case.result.status"]} {instrument.Attributes["test.suite.name"]}"),
            SequenceOrder.InAnyOrder);
        Assert.AreSequenceEqual([(unit, kind)], series.Select(static instrument => (instrument.Unit, instrument.Kind)).Distinct());
    }

    [TestMethod]
    public void Instrument_of_the_running_tests_goes_up_and_down()
    {
        var active = Assert.ContainsSingle(_instruments.Where(static instrument => instrument.Name == "test.case.active"));

        Assert.AreEqual("{test}", active.Unit);
        Assert.AreEqual(MetricType.LongSumNonMonotonic, active.Kind);
    }

    [TestMethod]
    public void Instrument_of_the_run_has_the_status_and_the_exit_code()
    {
        var run = Assert.ContainsSingle(_instruments.Where(static instrument => instrument.Name == "test.run.duration"));

        Assert.AreEqual("s", run.Unit);
        Assert.AreEqual(MetricType.Histogram, run.Kind);
        Assert.AreEqual("fail", run.Attributes["test.run.result.status"]);
        Assert.AreEqual("2", run.Attributes["test.run.exit_code"]);
    }
}
