namespace MSTest.SourceGeneration.Library.Inheritance;

/// <summary>
/// The top of a class hierarchy that a test class of another assembly derives from. Its test is public, and
/// <see cref="ParentTests"/> hides it with a member that the other assembly cannot access.
/// </summary>
public abstract class GrandparentTests
{
    [TestMethod]
    public void Test_that_the_parent_hides_is_called_through_reflection()
        => Assert.Contains("System.Reflection", Environment.StackTrace);
}
