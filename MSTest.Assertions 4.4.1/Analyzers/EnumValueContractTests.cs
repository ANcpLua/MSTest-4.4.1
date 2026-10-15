namespace MSTest.Assertions.Analyzers;

internal enum DumpType
{
    All = -1,
    Mini = 1,
    Heap = 2
}

internal enum Capacity : long
{
    Unlimited = -1,
    Terabyte = 1_099_511_627_776
}

/// <summary>
///     Pin the number behind an enum member: convert the member to the underlying type of its enum and compare it
///     with a numeric literal. MSTEST0032 accepts this assertion and reports other comparisons of two constants: the
///     two forms between the pragma lines are reported, and the build fails without the pragma.
/// </summary>
[TestClass]
public sealed class EnumValueContractTests
{
    [TestMethod]
    public void The_numbers_behind_the_members_are_pinned()
    {
        Assert.AreEqual(-1, (int)DumpType.All);
        Assert.AreEqual(1, (int)DumpType.Mini);
        Assert.AreEqual(2, (int)DumpType.Heap);

        Assert.AreEqual(-1L, (long)Capacity.Unlimited);
        Assert.AreEqual(1_099_511_627_776, (long)Capacity.Terabyte);
    }

    [TestMethod]
    public void Conversion_to_another_type()
    {
#pragma warning disable MSTEST0032
        Assert.AreEqual(1, (long)DumpType.Mini);
#pragma warning restore MSTEST0032
    }

    [TestMethod]
    public void Constant_in_place_of_the_member()
    {
        const DumpType selected = DumpType.Heap;

#pragma warning disable MSTEST0032
        Assert.AreEqual(2, (int)selected);
#pragma warning restore MSTEST0032
    }
}