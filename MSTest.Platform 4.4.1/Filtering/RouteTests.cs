namespace MSTest.Platform.Filtering;

/// <summary>
/// Three routes, each planned by one test. <see cref="FilterProviderRunTests"/> runs them with the filter
/// providers of this project, which select tests by their fully qualified name.
/// </summary>
[TestClass]
public sealed class RouteTests
{
    public const string ByRoad = $"MSTest.Platform.Filtering.{nameof(RouteTests)}.{nameof(Plans_a_route_by_road)}";
    public const string ByRail = $"MSTest.Platform.Filtering.{nameof(RouteTests)}.{nameof(Plans_a_route_by_rail)}";
    public const string BySea = $"MSTest.Platform.Filtering.{nameof(RouteTests)}.{nameof(Plans_a_route_by_sea)}";

    [TestMethod]
    public void Plans_a_route_by_road() => Assert.AreEqual(9, HoursFor(kilometres: 720, kilometresAnHour: 80));

    [TestMethod]
    public void Plans_a_route_by_rail() => Assert.AreEqual(6, HoursFor(kilometres: 720, kilometresAnHour: 120));

    [TestMethod]
    public void Plans_a_route_by_sea() => Assert.AreEqual(24, HoursFor(kilometres: 720, kilometresAnHour: 30));

    private static int HoursFor(int kilometres, int kilometresAnHour) => kilometres / kilometresAnHour;
}
