using MSTest.SourceGenerated;
using MSTest.SourceGeneration.Library.Inheritance;

namespace MSTest.SourceGeneration.InheritedMembers;

/// <summary>
/// A test class whose base classes come from another assembly. The generated registry is no part of the
/// class hierarchy, so it has an entry for each inherited member that is public and leaves out the internal
/// and the protected internal members of the base class. It also leaves out the inherited test that the base
/// class hides with such a member. That test still runs: MSTest finds it by looking the methods up and calls
/// it through reflection, as the test itself asserts in <see cref="GrandparentTests"/>.
/// </summary>
[TestClass]
public sealed class InheritedMemberTests : ParentTests
{
    private static IEnumerable<string> Methods
        => MSTestReflectionMetadata.TestClasses.Single(static entry => entry.Type == typeof(InheritedMemberTests))
            .Methods.Select(static method => method.Name);

    [TestMethod]
    public void Registry_lists_the_public_members_of_the_base_class()
    {
        Assert.Contains(nameof(Accessible_test_is_called_by_the_generated_registry), Methods);
        Assert.Contains(nameof(Accessible), Methods);
    }

    [TestMethod]
    public void Registry_leaves_out_the_internal_and_the_protected_internal_members()
    {
        Assert.DoesNotContain("Internal", Methods);
        Assert.DoesNotContain(nameof(ProtectedInternal), Methods);
    }

    [TestMethod]
    public void Registry_leaves_out_the_test_that_the_base_class_hides()
        => Assert.DoesNotContain(nameof(GrandparentTests.Test_that_the_parent_hides_is_called_through_reflection), Methods);
}
