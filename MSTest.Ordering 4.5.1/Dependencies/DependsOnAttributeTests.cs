using MSTest.Showcase;

namespace MSTest.Ordering.Dependencies;

/// <summary>
/// The constructors of DependsOnAttribute store the prerequisite in TestClass and TestMethodName.
/// ProceedOnFailure is false until it is set.
/// </summary>
[TestClass]
[Explicit]
[TestCategory("OrderingScenario")]
public sealed class DependsOnAttributeTests
{
    private const string CreateOrder = nameof(SameClassTests.CreateOrder);

    [TestMethod]
    public void Method_name_alone_leaves_the_class_open()
    {
        var dependency = new DependsOnAttribute(CreateOrder);

        Assert.IsNull(dependency.TestClass);
        Assert.AreEqual(CreateOrder, dependency.TestMethodName);
    }

    [TestMethod]
    public void Type_alone_leaves_the_method_open()
    {
        var dependency = new DependsOnAttribute(typeof(SameClassTests));

        Assert.AreEqual(typeof(SameClassTests), dependency.TestClass);
        Assert.IsNull(dependency.TestMethodName);
    }

    [TestMethod]
    public void Type_and_method_name_are_both_stored()
    {
        var dependency = new DependsOnAttribute(typeof(SameClassTests), CreateOrder);

        Assert.AreEqual(typeof(SameClassTests), dependency.TestClass);
        Assert.AreEqual(CreateOrder, dependency.TestMethodName);
    }

    [TestMethod]
    public void ProceedOnFailure_is_false_until_it_is_set()
    {
        Assert.IsFalse(new DependsOnAttribute(CreateOrder).ProceedOnFailure);
        Assert.IsTrue(new DependsOnAttribute(CreateOrder) { ProceedOnFailure = true }.ProceedOnFailure);
    }
}
