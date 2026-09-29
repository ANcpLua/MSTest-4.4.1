namespace MSTest.SourceGeneration.Descriptors;

/// <summary>
/// What a listing of <see cref="FallbackShapeTests"/> shows about the descriptors. MSTest takes the tests
/// with a descriptor first and looks the remaining ones up afterwards, so <c>--list-tests</c> names the
/// tests with a descriptor before the others, whatever their order in the class.
/// </summary>
[TestClass]
public sealed class DiscoveryOrderTests(TestContext testContext)
{
    [TestMethod]
    public async Task Listing_names_the_tests_with_a_descriptor_first()
    {
        var run = await TestApplicationRun.StartAsync(
            testContext.CancellationToken,
            "--list-tests",
            "--filter", $"ClassName={typeof(FallbackShapeTests).FullName}");

        Assert.AreSequenceEqual(
            [
                nameof(FallbackShapeTests.Supported_test_of_the_same_class_keeps_its_descriptor),
                nameof(FallbackShapeTests.Supported_test_with_a_data_row_keeps_its_descriptor),
                nameof(FallbackShapeTests.Descriptors_of_the_class_are_incomplete),
                nameof(FallbackShapeTests.Asynchronous_test_has_no_descriptor),
                nameof(FallbackShapeTests.Test_that_returns_a_task_has_no_descriptor),
                nameof(FallbackShapeTests.Test_with_dynamic_data_has_no_descriptor),
                nameof(FallbackShapeTests.Test_with_another_attribute_has_no_descriptor),
            ],
            run.Output.Split('\n')
                .Where(static line => line.StartsWith("  ", StringComparison.Ordinal) && !line.Contains(':'))
                .Select(static line => line.Trim().Split(' ')[0]));
        Assert.AreEqual(0, run.ExitCode);
    }
}
