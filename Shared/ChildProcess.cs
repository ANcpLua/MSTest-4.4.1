using System.Diagnostics;

namespace MSTest.Showcase;

/// <summary>
/// Owns a child process until it exits. Disposing after cancellation or a failed assertion terminates
/// the process tree and waits for the child to exit before releasing its handle.
/// </summary>
internal sealed class ChildProcess : IAsyncDisposable
{
    private ChildProcess(Process process) => Process = process;

    public Process Process { get; }

    public static ChildProcess Start(ProcessStartInfo start, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new ChildProcess(System.Diagnostics.Process.Start(start)
            ?? throw new InvalidOperationException("The child process could not be started."));
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (!Process.HasExited)
            {
                try
                {
                    Process.Kill(entireProcessTree: true);
                }
                catch (InvalidOperationException) when (Process.HasExited)
                {
                    // The child exited between checking its state and terminating it.
                }
            }

            await Process.WaitForExitAsync(CancellationToken.None);
        }
        finally
        {
            Process.Dispose();
        }
    }
}
