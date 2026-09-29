using System.Runtime.CompilerServices;

namespace MSTest.SourceGeneration.AsyncAttributes;

/// <summary>
/// A test method attribute that passes the attributes MSTest holds for the test method to the test, as its
/// only argument.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class TestMethodWithItsAttributesAttribute(
    [CallerFilePath] string declaringFilePath = "",
    [CallerLineNumber] int declaringLineNumber = -1)
    : TestMethodAttribute(declaringFilePath, declaringLineNumber)
{
    public override async Task<TestResult[]> ExecuteAsync(ITestMethod testMethod)
        => [await testMethod.InvokeAsync([testMethod.GetAllAttributes()!])];
}
