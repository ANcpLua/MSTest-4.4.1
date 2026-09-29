namespace MSTest.Execution.Context;

/// <summary>
/// A class that derives from <see cref="TestContext"/> supplies its own <see cref="TestContext.Properties"/>
/// dictionary. A directory property without an entry in that dictionary is <see langword="null"/>, although the
/// indexer of the dictionary throws for the same key. <see cref="TestContext.TestName"/> and
/// <see cref="TestContext.FullyQualifiedTestClassName"/> are not nullable and throw an
/// <see cref="InvalidOperationException"/> that names the property.
/// </summary>
[TestClass]
public sealed class MissingPropertyTests
{
    private readonly DictionaryTestContext _empty = new(new Dictionary<string, object?>());

    [TestMethod]
    public void A_directory_without_an_entry_is_null()
    {
        Assert.IsNull(_empty.TestRunDirectory);
        Assert.IsNull(_empty.DeploymentDirectory);
        Assert.IsNull(_empty.ResultsDirectory);
        Assert.IsNull(_empty.TestRunResultsDirectory);
        Assert.IsNull(_empty.TestResultsDirectory);
    }

    [TestMethod]
    public void The_dictionary_itself_throws_for_the_key_of_the_directory()
        => Assert.ThrowsExactly<KeyNotFoundException>(() => _empty.Properties[nameof(TestContext.TestRunDirectory)]);

    [TestMethod]
    public void A_directory_with_an_entry_is_that_entry()
    {
        DictionaryTestContext context = new(new Dictionary<string, object?>
        {
            [nameof(TestContext.TestRunDirectory)] = "/runs/42",
        });

        Assert.AreEqual("/runs/42", context.TestRunDirectory);
    }

    [TestMethod]
    public void A_name_without_an_entry_throws()
    {
        Exception testName = Assert.ThrowsExactly<InvalidOperationException>(() => _empty.TestName);
        Exception className = Assert.ThrowsExactly<InvalidOperationException>(() => _empty.FullyQualifiedTestClassName);

        Assert.Contains("'TestContext.TestName'", testName.Message);
        Assert.Contains("'TestContext.FullyQualifiedTestClassName'", className.Message);
    }
}
