using Microsoft.VisualStudio.TestTools.UnitTesting.Combinatorial;

namespace MSTest.Data.Combinatorial;

/// <summary>
///     <see cref="CombinatorialDataAttribute" /> asks a custom <see cref="ICombinatorialValuesProvider" /> for the
///     values of its parameter and combines them with the values of the other parameters.
/// </summary>
[TestClass]
public sealed class CustomValuesProviderTests
{
    [TestMethod]
    [CombinatorialData]
    public void Runs_for_the_values_of_the_provider([MembersExcept(Priority.Low)] Priority priority, bool express)
    {
        Assert.IsTrue(priority is Priority.Normal or Priority.High);
    }

    [TestMethod]
    public void The_values_of_the_provider_are_combined_with_the_others()
    {
        Assert.AreSequenceEqual(
            [
                (Priority.Normal, true), (Priority.Normal, false),
                (Priority.High, true), (Priority.High, false)
            ],
            new CombinatorialDataAttribute()
                .GetData(typeof(CustomValuesProviderTests).GetMethod(nameof(Runs_for_the_values_of_the_provider))!)
                .Select(row => ((Priority)row[0]!, (bool)row[1]!)));
    }
}