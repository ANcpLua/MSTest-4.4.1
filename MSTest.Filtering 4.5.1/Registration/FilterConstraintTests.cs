using System.Reflection;

namespace MSTest.Filtering.Registration;

/// <summary>
/// The type parameter of <c>TestFilterProviderAttribute&lt;TFilter&gt;</c> is constrained to
/// <see cref="ITestFilter"/> and <c>new()</c>. The compiler rejects a class that does not implement the interface
/// with CS0311, a struct that does not implement it with CS0315, and an abstract type or a type without a public
/// parameterless constructor with CS0310. A struct that implements the interface is accepted, because the type
/// parameter has no <c>class</c> constraint.
/// </summary>
[TestClass]
public sealed class FilterConstraintTests
{
    private static readonly Type Filter = typeof(TestFilterProviderAttribute<>).GetGenericArguments().Single();

    [TestMethod]
    public void The_type_parameter_requires_ITestFilter()
        => Assert.AreSame(typeof(ITestFilter), Filter.GetGenericParameterConstraints().Single());

    [TestMethod]
    public void The_type_parameter_requires_a_public_parameterless_constructor_and_allows_a_struct()
        => Assert.AreEqual(
            GenericParameterAttributes.DefaultConstructorConstraint,
            Filter.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask);
}
