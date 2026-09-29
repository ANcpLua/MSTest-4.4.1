namespace MSTest.Execution.Context;

/// <summary>
/// A <see cref="TestContext"/> whose <see cref="TestContext.Properties"/> are the dictionary it is given.
/// </summary>
public sealed class DictionaryTestContext(IDictionary<string, object?> properties) : TestContext
{
    public override IDictionary<string, object?> Properties => properties;

    public override void AddResultFile(string fileName)
    {
    }

    public override void Write(string? message)
    {
    }

    public override void Write(string format, params object?[] args)
    {
    }

    public override void WriteLine(string? message)
    {
    }

    public override void WriteLine(string format, params object?[] args)
    {
    }

    public override void DisplayMessage(MessageLevel messageLevel, string message)
    {
    }
}
