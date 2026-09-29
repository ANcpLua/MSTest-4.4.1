using MSTest.Showcase;

namespace MSTest.Platform.GitHubActions;

/// <summary>
/// A failure with a message of forty lines, one for each missing parcel, for the job summary that clips long
/// messages. Expected outcome with MSTEST_EXPLICIT=1 and a filter on this class: the test fails and the run
/// ends with exit code 2.
/// </summary>
[TestClass]
[Explicit]
public sealed class ManifestTests
{
    private static readonly int[] Missing = [.. Enumerable.Range(1, 40)];

    [TestMethod]
    public void Finds_every_parcel_of_the_manifest() =>
        Assert.Fail(string.Join('\n', Missing.Select(static parcel => $"Parcel {parcel} is missing.")));
}
