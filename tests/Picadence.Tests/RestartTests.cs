namespace Picadence.Tests;

public class RestartTests : IDisposable
{
    private static readonly DateTimeOffset Now = new(2026, 11, 22, 20, 0, 0, TimeSpan.Zero);
    private const string SessionId = "01a1fe3b-e1c8-71eb-afd8-f796496f0f78";

    private readonly string _root = Path.Combine(
        Path.GetTempPath(), "picadence-transcripts-" + Guid.NewGuid().ToString("N"));

    private readonly string _folder;

    private readonly string _sessions;

    private readonly string _transcript;

    private readonly string _terminal;

    private readonly string _shim;

    private readonly string _exe;

    public RestartTests()
    {
        _folder = Path.Combine(_root, "my project");
        _sessions = Path.Combine(_root, "sessions");
        var project = Path.Combine(_sessions, "--C--Users-me-my-project--");
        Directory.CreateDirectory(_folder);
        Directory.CreateDirectory(project);

        _transcript = Path.Combine(project, "2026-11-22T09-14-02-118Z_" + SessionId + ".jsonl");
        File.WriteAllText(_transcript, "{}");

        _terminal = Path.Combine(_root, "wt.exe");
        File.WriteAllText(_terminal, string.Empty);

        // Both shapes pi can be installed in: the .cmd shim it ships on Windows, and
        // an executable a future installer might bring.
        _shim = Path.Combine(_root, "pi.cmd");
        _exe = Path.Combine(_root, "pi.exe");
        File.WriteAllText(_shim, string.Empty);
        File.WriteAllText(_exe, string.Empty);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_root, recursive: true);
        }
        catch (IOException)
        {
            // A leaked temp folder is not a test failure.
        }
    }

    private Pin Pin(string? transcript, string folder) =>
        new(SessionId, "trayclaw", folder, transcript, Now);

    private RestartPlan Plan(
        Pin pin,
        bool running = false,
        string? terminal = null,
        string? pi = "shim") =>
        Restart.Plan(
            pin,
            running,
            terminal ?? _terminal,
            pi switch
            {
                "shim" => _shim,
                "exe" => _exe,
                "none" => null,
                _ => pi,
            },
            File.Exists,
            Directory.Exists,
            _sessions);

    [Fact(DisplayName = "restarting a kept session resumes that session")]
    public void Restarting_a_kept_session_resumes_that_session()
    {
        var plan = Plan(Pin(_transcript, _folder));

        Assert.True(plan.CanRestart);
        Assert.Equal(RestartRefusal.None, plan.Refusal);
        var launch = Assert.IsType<Launch>(plan.Launch);

        Assert.Equal(_terminal, launch.File);
        Assert.Equal(_folder, launch.WorkingDirectory);

        // The tab: one new tab, in the session's folder, named after the session.
        Assert.Contains($"{_terminal} -w 0 new-tab --title trayclaw", launch.CommandLine, StringComparison.Ordinal);
        Assert.Contains($"--startingDirectory \"{_folder}\"", launch.CommandLine, StringComparison.Ordinal);

        // The command: pi is a .cmd shim, so cmd.exe runs it, which is what the user's
        // own shell does. CreateProcess cannot start a batch file itself, and cmd
        // strips the outer quotes of a /c payload, so the payload arrives intact.
        Assert.Contains(
            $"-- {Restart.ComSpec()} /d /c \"{_shim} --session {Restart.Quote(_transcript)}\"",
            launch.CommandLine,
            StringComparison.Ordinal);
    }

    [Fact(DisplayName = "a real pi executable is started directly")]
    public void A_real_pi_executable_is_started_directly()
    {
        var launch = Assert.IsType<Launch>(Plan(Pin(_transcript, _folder), pi: "exe").Launch);

        Assert.DoesNotContain("cmd.exe", launch.CommandLine, StringComparison.OrdinalIgnoreCase);
        Assert.Contains($"-- {_exe} --session {_transcript}", launch.CommandLine, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "a machine without pi says so")]
    public void A_machine_without_pi_says_so()
    {
        var plan = Plan(Pin(_transcript, _folder), pi: "none");

        Assert.Equal(RestartRefusal.NoPi, plan.Refusal);
        Assert.Equal("pi is not installed", plan.Note);
        Assert.Null(plan.Launch);
    }

    [Fact(DisplayName = "a running session offers no restart")]
    public void A_running_session_offers_no_restart()
    {
        var plan = Plan(Pin(_transcript, _folder), running: true);

        Assert.False(plan.CanRestart);
        Assert.Null(plan.Launch);
        Assert.Equal(RestartRefusal.Running, plan.Refusal);
        Assert.Equal("already running", plan.Note);
    }

    [Fact(DisplayName = "a missing conversation is named")]
    public void A_missing_conversation_is_named()
    {
        var plan = Plan(Pin(Path.Combine(_sessions, "nowhere.jsonl"), _folder));

        Assert.Equal(RestartRefusal.NoConversation, plan.Refusal);
        Assert.Equal("the conversation is gone", plan.Note);
        Assert.Null(plan.Launch);
    }

    [Fact(DisplayName = "a conversation outside pi's sessions is not a conversation")]
    public void A_conversation_outside_pis_sessions_is_not_a_conversation()
    {
        var stray = Path.Combine(_folder, "not-a-transcript.jsonl");
        File.WriteAllText(stray, "{}");

        var plan = Plan(Pin(stray, _folder));

        Assert.Equal(RestartRefusal.NoConversation, plan.Refusal);
    }

    [Fact(DisplayName = "a missing folder is named")]
    public void A_missing_folder_is_named()
    {
        var plan = Plan(Pin(_transcript, Path.Combine(_root, "gone")));

        Assert.Equal(RestartRefusal.NoFolder, plan.Refusal);
        Assert.Equal("the folder is gone", plan.Note);
    }

    [Fact(DisplayName = "a missing terminal is named")]
    public void A_missing_terminal_is_named()
    {
        var plan = Restart.Plan(
            Pin(_transcript, _folder),
            running: false,
            terminal: null,
            pi: _shim,
            File.Exists,
            Directory.Exists,
            _sessions);

        Assert.Equal(RestartRefusal.NoTerminal, plan.Refusal);
        Assert.Equal("windows terminal is not installed", plan.Note);
    }

    [Fact(DisplayName = "a folder with a space survives the command line")]
    public void A_folder_with_a_space_survives_the_command_line()
    {
        var launch = Assert.IsType<Launch>(Plan(Pin(_transcript, _folder)).Launch);

        Assert.Contains($"\"{_folder}\"", launch.CommandLine, StringComparison.Ordinal);
        Assert.Contains("--", launch.CommandLine, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "a path that ends in a backslash keeps its quote")]
    public void A_path_that_ends_in_a_backslash_keeps_its_quote()
    {
        // Only an argument that needs quoting is quoted, and the backslashes that
        // would otherwise swallow the closing quote are doubled.
        Assert.Equal("\"C:\\my tools\\\\\"", Restart.Quote(@"C:\my tools\"));
        Assert.Equal("\"a b\"", Restart.Quote("a b"));
        Assert.Equal("plain", Restart.Quote("plain"));
    }

    [Fact(DisplayName = "a name cannot write to the terminal through a title")]
    public void A_name_cannot_write_to_the_terminal_through_a_title()
    {
        var pin = new Pin(SessionId, "bad\u001b]0;owned\u0007\"", "C:\\x", null, Now);

        var title = Restart.Title(pin);

        Assert.StartsWith("bad]0;owned", title, StringComparison.Ordinal);
        Assert.DoesNotContain(title, char.IsControl);
        Assert.DoesNotContain('"', title);
    }

    [Fact(DisplayName = "a session without a name takes its folder's last segment")]
    public void A_session_without_a_name_takes_its_folders_last_segment()
    {
        var pin = new Pin(SessionId, null, @"C:\Users\me\my project\", null, Now);

        Assert.Equal("my project", Restart.Title(pin));
    }

    [Fact(DisplayName = "a restarted session keeps what was armed for it")]
    public void A_restarted_session_keeps_what_was_armed_for_it()
    {
        // Armed while the session was live under its old process, then restarted:
        // the record is keyed by session id, so a new pid finds it again.
        var armed = new Schedule(SessionId, "trayclaw", _folder, Cadence.Hourly, Now.AddMinutes(30), null, 0);
        var restarted = new AgentSession(
            new SessionRef(4242, SessionId), _folder, "trayclaw", null, AgentState.Idle, null, Now.AddMinutes(-1));

        Assert.True(Plan(Pin(_transcript, _folder)).CanRestart);

        var tick = SchedulePlan.Plan([restarted], [armed], Now);

        Assert.True(tick.View.ContainsKey(restarted.Reference));
        Assert.Equal(Now.AddMinutes(30), tick.View[restarted.Reference].NextAt);
        Assert.Empty(tick.Pings);
    }

    [Fact(DisplayName = "the tray writes nothing into pi's storage")]
    public void The_tray_writes_nothing_into_pis_storage()
    {
        var lanes = new[] { Paths.Agents, Paths.Schedules, Paths.Pings, Paths.Pins };

        Assert.All(lanes, lane => Assert.StartsWith(Paths.Root, lane, StringComparison.OrdinalIgnoreCase));
        Assert.All(
            lanes,
            lane => Assert.DoesNotContain(
                Path.TrimEndingDirectorySeparator(Paths.PiSessions) + Path.DirectorySeparatorChar,
                lane,
                StringComparison.OrdinalIgnoreCase));
    }

    [Fact(DisplayName = "a conversation is found in any project folder")]
    public void A_conversation_is_found_in_any_project_folder()
    {
        Assert.Equal(_transcript, Transcripts.Find(_sessions, SessionId));
        Assert.Null(Transcripts.Find(_sessions, "another-session"));
        Assert.Null(Transcripts.Find(Path.Combine(_root, "missing"), SessionId));
    }

    [Fact(DisplayName = "the tray starts nothing the user did not ask for")]
    public void The_tray_starts_nothing_the_user_did_not_ask_for()
    {
        // One place calls CreateProcess: the method the panel asks when the user
        // picks Restart. Everything else that reaches the launcher is a decision
        // value, not a start.
        var source = Path.Combine(RepoRoot(), "src", "Picadence");
        var callers = Directory
            .EnumerateFiles(source, "*.cs")
            .Where(file => File.ReadAllText(file).Contains("CreateProcessW", StringComparison.Ordinal))
            .Select(file => Path.GetFileName(file))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(["Native.cs", "Program.cs"], callers);
        Assert.Single(
            File.ReadAllLines(Path.Combine(source, "Program.cs")),
            line => line.Contains("Native.CreateProcessW(", StringComparison.Ordinal));
        Assert.Single(
            File.ReadAllLines(Path.Combine(source, "Panel.cs")),
            line => line.Contains("Launcher(launch)", StringComparison.Ordinal));
    }

    /// <summary>The folder the solution sits in, found by walking up from the test binary.</summary>
    private static string RepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "picadence.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("no repository root above " + AppContext.BaseDirectory);
    }
}
