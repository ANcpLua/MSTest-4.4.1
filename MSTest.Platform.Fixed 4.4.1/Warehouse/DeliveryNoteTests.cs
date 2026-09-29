namespace MSTest.Platform.Fixed.Warehouse;

/// <summary>
/// Two tests that attach a delivery note to their result with <c>TestContext.AddResultFile</c>, one a file that
/// exists and one a file that was never written, for the run of <see cref="Reports.AttachmentRunTests"/>.
/// </summary>
[TestClass]
public sealed class DeliveryNoteTests(TestContext testContext)
{
    public const string SignedNote = "signed-delivery-note.txt";

    public static readonly string UnprintedNote = Path.Combine(Path.GetTempPath(), "showcase-unprinted-delivery-note.txt");

    [TestMethod]
    public void Attaches_the_signed_delivery_note()
    {
        var note = Path.Combine(Directory.CreateTempSubdirectory().FullName, SignedNote);
        File.WriteAllText(note, "Signed by the customer.");

        testContext.AddResultFile(note);

        Assert.IsTrue(File.Exists(note));
    }

    [TestMethod]
    public void Attaches_the_note_that_was_never_printed()
    {
        testContext.AddResultFile(UnprintedNote);

        Assert.IsFalse(File.Exists(UnprintedNote));
    }
}
