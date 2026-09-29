namespace MSTest.Isolation.ResourceLocks;

/// <summary>
/// The resources the tests of this assembly share. Two locks mean the same resource when their keys are equal
/// character by character, so every test names a constant of this class and none spells a key again.
/// </summary>
internal static class SharedResources
{
    public const string Ledger = "ledger";

    public const string Vault = "vault";

    public const string Catalog = "catalog";

    public const string Inventory = "inventory";

    public const string CheckingAccount = "checking-account";

    public const string SavingsAccount = "savings-account";

    public const string Teller = "teller";

    public const string Printer = "printer";

    public const string Database = "database";

    public const string Scanner = "scanner";

    public const string ScannerInCapitals = "SCANNER";
}
