namespace MSTest.Analyzers.LegacyBase;

/// <summary>
/// A contract of a library that is compiled against MSTest.TestFramework 3.11.1: the tests that every
/// normalizer passes, for the test class of each normalizer to inherit.
/// </summary>
public abstract class LegacyNormalizerContract
{
    public static int Runs { get; private set; }

    protected abstract string Normalize(string text);

    [TestMethod]
    public void Keeps_an_empty_text()
    {
        Runs++;

        Assert.AreEqual(string.Empty, Normalize(string.Empty));
    }

#pragma warning disable MSTEST0044
    [DataTestMethod]
#pragma warning restore MSTEST0044
    [DataRow(" padded ")]
    [DataRow("plain")]
    public void Changes_nothing_the_second_time(string text)
    {
        Runs++;

        string normalized = Normalize(text);

        Assert.AreEqual(normalized, Normalize(normalized));
    }
}
