using System.Globalization;
using MSTest.Isolation.ClassLevel.ResourceLocks;

namespace MSTest.Isolation.ClassLevel.Analyzers;

/// <summary>
///     MSTEST0076 reports a test that assigns CultureInfo.DefaultThreadCurrentCulture or
///     CultureInfo.DefaultThreadCurrentUICulture, the defaults of the whole process. It accepts the assignment in
///     a test with [DoNotParallelize] and in a test with a [ResourceLock] of any key, and it accepts
///     CultureInfo.CurrentCulture, which changes the culture of the test alone.
/// </summary>
[TestClass]
public sealed class DefaultCultureRuleTests(TestContext testContext)
{
    private static readonly CultureInfo Austrian = CultureInfo.GetCultureInfo("de-AT");

    [TestMethod]
    [DoNotParallelize]
    public async Task Default_culture_assigned_outside_the_parallel_run_is_accepted()
    {
        var previous = CultureInfo.DefaultThreadCurrentCulture;
        CultureInfo.DefaultThreadCurrentCulture = Austrian;

        try
        {
            Assert.AreEqual("1,5", await Task.Run(FormatOneAndAHalf, testContext.CancellationToken));
        }
        finally
        {
            CultureInfo.DefaultThreadCurrentCulture = previous;
        }
    }

    [TestMethod]
    [ResourceLock(SharedResources.DefaultCulture)]
    public async Task Default_culture_assigned_under_a_lock_is_accepted()
    {
        var previous = CultureInfo.DefaultThreadCurrentCulture;
        CultureInfo.DefaultThreadCurrentCulture = Austrian;

        try
        {
            Assert.AreEqual("1,5", await Task.Run(FormatOneAndAHalf, testContext.CancellationToken));
        }
        finally
        {
            CultureInfo.DefaultThreadCurrentCulture = previous;
        }
    }

    [TestMethod]
    public void Culture_of_the_test_alone_is_accepted()
    {
        CultureInfo.CurrentCulture = Austrian;

        Assert.AreEqual("1,5", FormatOneAndAHalf());
        Assert.IsNull(CultureInfo.DefaultThreadCurrentCulture);
    }

    private static string FormatOneAndAHalf()
    {
        return 1.5.ToString("F1", CultureInfo.CurrentCulture);
    }

#pragma warning disable MSTEST0076
    [TestMethod]
    public async Task Default_culture_assigned_without_a_lock_is_reported()
    {
        var previous = CultureInfo.DefaultThreadCurrentCulture;
        CultureInfo.DefaultThreadCurrentCulture = Austrian;

        try
        {
            Assert.AreEqual("1,5", await Task.Run(FormatOneAndAHalf, testContext.CancellationToken));
        }
        finally
        {
            CultureInfo.DefaultThreadCurrentCulture = previous;
        }
    }

    [TestMethod]
    public void Default_interface_culture_assigned_without_a_lock_is_reported()
    {
        var previous = CultureInfo.DefaultThreadCurrentUICulture;
        CultureInfo.DefaultThreadCurrentUICulture = Austrian;

        try
        {
            Assert.AreEqual(Austrian, CultureInfo.CurrentUICulture);
        }
        finally
        {
            CultureInfo.DefaultThreadCurrentUICulture = previous;
        }
    }
#pragma warning restore MSTEST0076
}