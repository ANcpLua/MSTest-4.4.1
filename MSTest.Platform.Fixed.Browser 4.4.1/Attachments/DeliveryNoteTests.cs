namespace MSTest.Platform.Fixed.Browser.Attachments;

/// <summary>
/// Attaches a signed delivery note from the browser runtime's virtual file system. The simplified console
/// output names the attachment so a host can identify files produced by a browser-wasm test.
/// </summary>
[TestClass]
public sealed class DeliveryNoteTests(TestContext testContext)
{
    [TestMethod]
    public void Attaches_the_signed_delivery_note()
    {
        var path = Path.Combine(testContext.TestTempDirectory!, "signed-delivery-note.txt");
        const string contents = "Signed by the customer.";
        File.WriteAllText(path, contents);

        testContext.AddResultFile(path);

        Assert.AreEqual(contents, File.ReadAllText(path));
    }
}
