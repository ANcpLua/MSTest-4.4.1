using System.Globalization;
using MSTest.Showcase;

namespace MSTest.Platform.Fixed.Warehouse;

/// <summary>
/// A test that fails with an <see cref="InvalidOperationException"/> around the <see cref="FormatException"/>
/// that it caught, for the runs of the examples that read a failure. Expected outcome with MSTEST_EXPLICIT=1
/// and a filter on this class: the test fails and the run ends with exit code 2.
/// </summary>
[TestClass]
[Explicit]
public sealed class DamagedParcelTests
{
    public const string Damaged = "The parcel is damaged.";

    [TestMethod]
    public void Rejects_the_damaged_parcel()
    {
        try
        {
            _ = int.Parse("twelve", CultureInfo.InvariantCulture);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException(Damaged, exception);
        }
    }
}
