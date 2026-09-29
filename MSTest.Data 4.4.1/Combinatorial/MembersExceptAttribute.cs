using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting.Combinatorial;

namespace MSTest.Data.Combinatorial;

/// <summary>
/// An attribute that implements <see cref="ICombinatorialValuesProvider"/> provides the values of the
/// parameter it is applied to. This one provides the members of the parameter's enum without the excluded ones.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class MembersExceptAttribute(params object[] excluded) : Attribute, ICombinatorialValuesProvider
{
    public object?[] GetValues(ParameterInfo parameter)
        => [.. Enum.GetValues(parameter.ParameterType).Cast<object>().Where(member => !excluded.Contains(member))];
}
