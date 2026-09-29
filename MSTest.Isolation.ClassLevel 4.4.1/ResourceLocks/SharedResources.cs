namespace MSTest.Isolation.ClassLevel.ResourceLocks;

/// <summary>
/// The resources the classes of this assembly share. Two locks mean the same resource when their keys are
/// equal character by character, so every class names a constant of this class and none spells a key again.
/// </summary>
internal static class SharedResources
{
    public const string Warehouse = "warehouse";

    public const string PriceList = "price-list";

    public const string DefaultCulture = "default-culture";
}
