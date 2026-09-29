using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting.Combinatorial;
using MSTest.Showcase;

namespace MSTest.Data.Combinatorial;

/// <summary>
/// Every parameter needs exactly one source of values. <see cref="CombinatorialDataAttribute.GetData"/> throws
/// <see cref="NotSupportedException"/> for a type without inferred values and without a provider, and
/// <see cref="ArgumentException"/> for a parameter with more than one provider.
/// </summary>
[TestClass]
public sealed class ValueProviderRulesTests
{
    [TestMethod]
    public void A_type_without_inferred_values_needs_a_provider()
    {
        MethodInfo test = typeof(MisconfiguredParameterTests)
            .GetMethod(nameof(MisconfiguredParameterTests.A_string_has_no_inferred_values))!;

        NotSupportedException exception = Assert.ThrowsExactly<NotSupportedException>(
            () => new CombinatorialDataAttribute().GetData(test));

        Assert.Contains(typeof(string).FullName!, exception.Message);
        Assert.Contains(nameof(ICombinatorialValuesProvider), exception.Message);
    }

    [TestMethod]
    public void A_parameter_takes_one_provider()
    {
        MethodInfo test = typeof(MisconfiguredParameterTests)
            .GetMethod(nameof(MisconfiguredParameterTests.Two_providers_compete_for_one_parameter))!;

        ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(
            () => new CombinatorialDataAttribute().GetData(test));

        Assert.Contains("parcels", exception.Message);
        Assert.Contains(
            $"{nameof(CombinatorialValuesAttribute)}, {nameof(CombinatorialRangeAttribute)}",
            exception.Message);
    }
}

/// <summary>
/// Both tests fail on purpose and run no row. The first reports that no values can be generated for a
/// parameter of type <c>System.String</c>, the second that parameter <c>parcels</c> has multiple combinatorial
/// value providers.
/// </summary>
[TestClass]
[Explicit]
public sealed class MisconfiguredParameterTests
{
    [TestMethod]
    [CombinatorialData]
    public void A_string_has_no_inferred_values(string country)
    {
    }

    [TestMethod]
    [CombinatorialData]
    public void Two_providers_compete_for_one_parameter(
        [CombinatorialValues(1)] [CombinatorialRange(1, 2)] int parcels)
    {
    }
}
