using System.Diagnostics;
using System.Runtime.CompilerServices;
using MSTest.SourceGenerated;

namespace MSTest.SourceGeneration.AsyncAttributes;

/// <summary>
/// The attributes that MSTest holds for an asynchronous test. The declared ones are the instances that the
/// generated registry constructs. The compiler marks an asynchronous method with
/// <see cref="AsyncStateMachineAttribute"/> and, in a Debug build, with
/// <see cref="DebuggerStepThroughAttribute"/>. The generated module initializer reads these two from the
/// method and appends them to the declared attributes. The declared attributes work as they do without the
/// generator: <c>[TestProperty]</c> reaches <see cref="TestContext.Properties"/>.
/// </summary>
[TestClass]
public sealed class AsyncAttributeTests(TestContext testContext)
{
    private const string Owner = nameof(Owner);

    [TestMethodWithItsAttributes]
    [TestCategory("Asynchronous")]
    [TestProperty(Owner, "Showcase")]
    public async Task Declared_attributes_are_the_instances_of_the_registry(Attribute[] attributes)
    {
        await Task.Yield();
        var declared = Declared(nameof(Declared_attributes_are_the_instances_of_the_registry));

        Assert.AreSequenceEqual(
            [typeof(TestMethodWithItsAttributesAttribute), typeof(TestCategoryAttribute), typeof(TestPropertyAttribute)],
            declared.Select(static attribute => attribute.GetType()));
        Assert.AreSequenceEqual(declared, attributes.Take(declared.Length), ReferenceEqualityComparer.Instance);
    }

    [TestMethodWithItsAttributes]
    [TestCategory("Asynchronous")]
    public async Task Attributes_of_the_compiler_are_appended_at_run_time(Attribute[] attributes)
    {
        await Task.Yield();
        var declared = Declared(nameof(Attributes_of_the_compiler_are_appended_at_run_time));
        var appended = attributes.Skip(declared.Length).Select(static attribute => attribute.GetType()).ToArray();

        Assert.Contains(typeof(AsyncStateMachineAttribute), appended);
        Assert.IsEmpty(appended.Except([typeof(AsyncStateMachineAttribute), typeof(DebuggerStepThroughAttribute)]));
    }

    [TestMethod]
    [TestProperty(Owner, "Showcase")]
    public async Task Declared_test_property_reaches_the_test_context()
    {
        await Task.Yield();

        Assert.AreEqual("Showcase", testContext.Properties[Owner]);
    }

    private static Attribute[] Declared(string test)
        => MSTestReflectionMetadata.TestClasses.Single(static entry => entry.Type == typeof(AsyncAttributeTests))
            .Methods.Single(method => method.Name == test)
            .Attributes;
}
