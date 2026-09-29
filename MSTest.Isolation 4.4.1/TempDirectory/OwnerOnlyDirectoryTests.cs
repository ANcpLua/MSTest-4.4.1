using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace MSTest.Isolation.TempDirectory;

/// <summary>
/// On Unix the TestTempDirectory is created with access for its owner alone: read, write and execute for the
/// user, nothing for the group and for others. The file mode mask of the process does not change that: under
/// a mask that denies every permission the test still writes into its directory.
/// </summary>
[TestClass]
public sealed class OwnerOnlyDirectoryTests(TestContext testContext)
{
    private const UnixFileMode OwnerOnly = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;

    private const uint EveryPermission = 0x1FF;

    [TestMethod]
    [OSCondition(ConditionMode.Exclude, OperatingSystems.Windows)]
    [UnsupportedOSPlatform("windows")]
    public void Grants_access_to_the_owner_alone()
    {
        Assert.AreEqual(OwnerOnly, File.GetUnixFileMode(testContext.TestTempDirectory!));
    }

    [TestMethod]
    [DoNotParallelize]
    [OSCondition(ConditionMode.Exclude, OperatingSystems.Windows)]
    [UnsupportedOSPlatform("windows")]
    public void Grants_the_same_access_under_a_mask_that_denies_everything()
    {
        uint previous = SetFileModeMask(EveryPermission);

        try
        {
            string directory = testContext.TestTempDirectory!;
            string note = Path.Combine(directory, "note.txt");

            File.WriteAllText(note, "written");

            Assert.AreEqual(OwnerOnly, File.GetUnixFileMode(directory));
            Assert.IsTrue(File.Exists(note));
        }
        finally
        {
            SetFileModeMask(previous);
        }
    }

    [DllImport("libc", EntryPoint = "umask")]
    private static extern uint SetFileModeMask(uint mask);
}
