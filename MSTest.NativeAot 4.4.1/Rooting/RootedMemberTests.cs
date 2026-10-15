using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace MSTest.NativeAot.Rooting;

/// <summary>
///     What the generator roots of a test class. It roots the constructors, methods, fields and properties, so
///     a private method that no code calls stays in the trimmed executable. It roots no nested type: the
///     trimmer removes the method of a nested helper that no code calls and the nested type that no code uses.
///     Both exist under <c>dotnet run</c>. A nested type that derives from <see cref="Exception" /> or
///     <see cref="Stream" /> causes no trim warning when the project is published.
/// </summary>
[TestClass]
public sealed class RootedMemberTests
{
    private const BindingFlags NonPublic = BindingFlags.NonPublic | BindingFlags.Instance;

    [TestMethod]
    public void Method_of_the_test_class_without_a_caller_stays_in_the_executable()
    {
        Assert.IsNotNull(Method(this, nameof(Reset)));
    }

    [TestMethod]
    public void Method_of_a_nested_type_without_a_caller_is_trimmed_from_the_native_executable()
    {
        Assert.AreEqual(NativeExecutable.IsRunning, Method(new Counter(), nameof(Counter.Reset)) is null);
    }

    [TestMethod]
    [DataRow(nameof(UnusedStream))]
    [DataRow(nameof(UnusedException))]
    [DataRow(nameof(UnusedState))]
    public void Nested_type_without_a_user_is_trimmed_from_the_native_executable(string name)
    {
        Assert.AreEqual(NativeExecutable.IsRunning, NestedType(this, name) is null);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2075",
        Justification = "The example looks up what the trimmer has left.")]
    private static MethodInfo? Method(object owner, string name)
    {
        return owner.GetType().GetMethod(name, NonPublic);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2075",
        Justification = "The example looks up what the trimmer has left.")]
    private static Type? NestedType(object owner, string name)
    {
        return owner.GetType().GetNestedType(name, BindingFlags.NonPublic);
    }

    private void Reset()
    {
        throw new NotSupportedException();
    }

    private sealed class Counter
    {
        internal void Reset()
        {
            throw new NotSupportedException();
        }
    }

    // Must remain uninstantiated so the trimming test can observe its removal.
    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class UnusedStream : MemoryStream;

    // Must remain uninstantiated so the trimming test can observe its removal.
    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class UnusedException : Exception;

    private enum UnusedState
    {
        // The containing enum is deliberately unused and should be trimmed.
        // ReSharper disable once UnusedMember.Local
        Ready
    }
}