// The bug, reproduced: the .NET CLI installs its command as a batch file, and a
// command shell waits for what a batch file starts, so the tray used to hold the
// terminal it was started from. This runs the same shape the CLI writes, around a
// real build of the tray, and fails while the shell is still waiting.
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Picadence.Tests;

public class StartupProcessTests
{
    /// <summary>Long enough for a slow cold start, short enough that a hang is the answer.</summary>
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(20);

    [Fact(DisplayName = "a start from a command shell leaves the shell free")]
    public void A_start_from_a_command_shell_leaves_the_shell_free()
    {
        var exe = TrayExe();
        Assert.True(File.Exists(exe), $"the test build of the app is not at {exe}");

        var before = DateTime.UtcNow;
        var folder = Directory.CreateTempSubdirectory("picadence-start-");
        try
        {
            // The same shape the CLI writes for a native tool: a batch that runs the exe.
            var shim = Path.Combine(folder.FullName, "picadence.cmd");
            File.WriteAllText(shim, $"@echo off\r\n\"{exe}\"\r\n");

            var start = new ProcessStartInfo("cmd.exe") { UseShellExecute = false, CreateNoWindow = true };
            start.ArgumentList.Add("/c");
            start.ArgumentList.Add(shim);

            using var shell = Process.Start(start);
            Assert.NotNull(shell);
            Assert.True(
                shell.WaitForExit(Timeout),
                "the shell was still waiting for the tray, which is the bug this test exists for");
        }
        finally
        {
            StopTheCopy(before);
            TryDelete(folder);
        }
    }

    /// <summary>
    /// The app's own output, not the copy beside the tests: that copy is an apphost for
    /// a self-contained build without the runtime beside it, so it cannot start.
    /// </summary>
    private static string TrayExe()
    {
        var tests = new DirectoryInfo(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar));
        var framework = tests.Name;
        var configuration = tests.Parent?.Name ?? "Release";

        var root = tests;
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "picadence.sln")))
        {
            root = root.Parent;
        }

        Assert.NotNull(root);
        return Path.Combine(
            root.FullName, "src", "Picadence", "bin", configuration, framework,
            RuntimeInformation.RuntimeIdentifier, "Picadence.exe");
    }

    /// <summary>
    /// The copy the test started becomes the tray when no other one is running, so it
    /// is stopped again. A tray that was already there has an earlier start time and is
    /// left alone.
    /// </summary>
    private static void StopTheCopy(DateTime before)
    {
        foreach (var process in Process.GetProcessesByName("Picadence"))
        {
            using (process)
            {
                try
                {
                    if (process.StartTime.ToUniversalTime() >= before)
                    {
                        process.Kill();
                    }
                }
                catch (InvalidOperationException)
                {
                    // It ended while the list was being read.
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    // It ended, or it is not ours to stop. Either way there is nothing left.
                }
            }
        }
    }

    private static void TryDelete(DirectoryInfo folder)
    {
        try
        {
            folder.Delete(recursive: true);
        }
        catch (IOException)
        {
            // A leftover in the temp folder is not worth failing a test over.
        }
        catch (UnauthorizedAccessException)
        {
            // Same.
        }
    }
}
