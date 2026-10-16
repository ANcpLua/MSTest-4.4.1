using System.Globalization;
using MSTest.SourceGenerated;

namespace MSTest.SourceGeneration.Literals;

/// <summary>
///     The generator writes the arguments of <c>[DataRow]</c> as C# literals into the registry, and the rows of a
///     test are the <see cref="DataRowAttribute" /> instances that the registry constructs from them. A literal
///     keeps the type and the value of the argument: <see cref="byte" />, <see cref="sbyte" />,
///     <see cref="short" /> and <see cref="ushort" /> values, enumeration members with a negative value, control
///     characters, <c>NaN</c> and the infinities arrive as they are declared.
/// </summary>
[TestClass]
public sealed class LiteralTests
{
    [TestMethod]
    [DataRow((byte)200, typeof(byte))]
    [DataRow((sbyte)-100, typeof(sbyte))]
    [DataRow((short)-30000, typeof(short))]
    [DataRow((ushort)60000, typeof(ushort))]
    public void Narrow_integral_value_keeps_its_type(object value, Type type)
    {
        Assert.AreEqual(type, value.GetType());
    }

    [TestMethod]
    [DataRow(Direction.Backward, -1)]
    [DataRow(Direction.Forward, 1)]
    public void Enumeration_member_keeps_its_type(object value, int number)
    {
        Assert.IsInstanceOfType<Direction>(value);
        Assert.AreEqual(number, (int)(Direction)value);
    }

    [TestMethod]
    [DataRow('\0', 0)]
    [DataRow('\n', 10)]
    [DataRow('\u001b', 27)]
    public void Control_character_arrives_unchanged(char value, int code)
    {
        Assert.AreEqual(code, value);
    }

    [TestMethod]
    [DataRow(double.NaN, "NaN")]
    [DataRow(double.PositiveInfinity, "Infinity")]
    [DataRow(double.NegativeInfinity, "-Infinity")]
    public void Double_that_is_not_finite_arrives_unchanged(double value, string text)
    {
        Assert.AreEqual(text, value.ToString(CultureInfo.InvariantCulture));
    }

    [TestMethod]
    [DataRow(float.NaN, "NaN")]
    [DataRow(float.PositiveInfinity, "Infinity")]
    [DataRow(float.NegativeInfinity, "-Infinity")]
    public void Single_that_is_not_finite_arrives_unchanged(float value, string text)
    {
        Assert.AreEqual(text, value.ToString(CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void Rows_of_the_registry_hold_the_declared_types()
    {
        var rows = MSTestReflectionMetadata.TestClasses.Single(static entry => entry.Type == typeof(LiteralTests))
            .Methods.Single(static method => method.Name == nameof(Narrow_integral_value_keeps_its_type))
            .Attributes.OfType<DataRowAttribute>();

        Assert.AreSequenceEqual(
            [typeof(byte), typeof(sbyte), typeof(short), typeof(ushort)],
            rows.Select(static row => row.Data[0]!.GetType()));
    }
    [TestMethod]
    [DataRow(null)]
    public void Null_array_attribute_argument_remains_one_null_argument(object? value)
        => Assert.IsNull(value);

}