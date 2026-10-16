namespace MSTest.NativeAot.DynamicData;

/// <summary>
/// A data source in a class other than the test class. A test names it with
/// <c>[DynamicData(nameof(Catalog.Prices), typeof(Catalog))]</c>, and the generated registry reads it
/// through a delegate.
/// </summary>
public static class Catalog
{
    public static IEnumerable<object[]> Prices => [["Tea", 3], ["Coffee", 4]];
}
