namespace MSTest.SourceGeneration.Library.Inheritance;

/// <summary>
///     The base class of a test class of another assembly. Code of that assembly that is no part of the class
///     hierarchy can call the public members, and it cannot call the internal and the protected internal ones.
///     One of them hides the test of <see cref="GrandparentTests" />.
/// </summary>
public abstract class ParentTests : GrandparentTests
{
    [TestMethod]
    public void Accessible_test_is_called_by_the_generated_registry()
    {
        Assert.Contains("MSTest.SourceGenerated.MSTestReflectionMetadata", Environment.StackTrace);
    }

    public int Accessible()
    {
        return Internal() + ProtectedInternal();
    }

    // Instance visibility and hiding across assemblies are the contract under test.
#pragma warning disable CA1822
    // ReSharper disable MemberCanBePrivate.Global, MemberCanBeMadeStatic.Global, UnusedMember.Global
    internal int Internal()
    {
        return 1;
    }

    protected internal int ProtectedInternal()
    {
        return 2;
    }

    protected internal new void Test_that_the_parent_hides_is_called_through_reflection()
    {
    }
}
    // ReSharper restore MemberCanBePrivate.Global, MemberCanBeMadeStatic.Global, UnusedMember.Global
#pragma warning restore CA1822
