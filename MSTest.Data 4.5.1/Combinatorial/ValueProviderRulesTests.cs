using Microsoft.VisualStudio.TestTools.UnitTesting.Combinatorial;

namespace MSTest.Data.Combinatorial;

/// <summary>
///     Every parameter needs exactly one source of values. <see cref="CombinatorialDataAttribute.GetData" /> throws
///     <see cref="NotSupportedException" /> for a type without inferred values and without a provider, and
///     <see cref="ArgumentException" /> for a parameter with more than one provider.
/// </summary>
[TestClass]
public sealed class ValueProviderRulesTests
{
    [TestMethod]
    public void A_type_without_inferred_values_needs_a_provider()
    {
        var test = typeof(MisconfiguredParameterExamples)
            .GetMethod(nameof(MisconfiguredParameterExamples.A_string_has_no_inferred_values))!;

        var exception =
            Assert.ThrowsExactly<NotSupportedException>(() => new CombinatorialDataAttribute().GetData(test));

        Assert.Contains(typeof(string).FullName!, exception.Message);
        Assert.Contains(nameof(ICombinatorialValuesProvider), exception.Message);
    }

    [TestMethod]
    public void A_parameter_takes_one_provider()
    {
        var test = typeof(MisconfiguredParameterExamples)
            .GetMethod(nameof(MisconfiguredParameterExamples.Two_providers_compete_for_one_parameter))!;

        var exception = Assert.ThrowsExactly<ArgumentException>(() => new CombinatorialDataAttribute().GetData(test));

        Assert.Contains("parcels", exception.Message);
        Assert.Contains(
            $"{nameof(CombinatorialValuesAttribute)}, {nameof(CombinatorialRangeAttribute)}",
            exception.Message);
    }
}

// Method metadata for GetData rejection tests. These are inputs, not executable test cases.
internal static class MisconfiguredParameterExamples
{
    public static void A_string_has_no_inferred_values(string country) => throw new NotSupportedException($"Metadata-only fixture: {country}");

    public static void Two_providers_compete_for_one_parameter(
        [CombinatorialValues(1)] [CombinatorialRange(1, 2)] int parcels) => throw new NotSupportedException($"Metadata-only fixture: {parcels}");
}
