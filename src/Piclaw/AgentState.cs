namespace Piclaw;

/// <summary>
/// The states a session can be in. The names match the ones pi reports to the
/// terminal, so a reader learns the vocabulary once.
/// </summary>
public enum AgentState
{
    /// <summary>Pi is up and nobody is running anything, or a run was cancelled.</summary>
    Idle,

    /// <summary>A run finished. Nothing is expected from the user.</summary>
    Done,

    /// <summary>An agent run is in progress.</summary>
    Working,

    /// <summary>Pi is waiting for the user to answer something.</summary>
    Blocked,

    /// <summary>A run ended in a failure that pi did not retry.</summary>
    Error,
}

public static class AgentStateExtensions
{
    /// <summary>Reads the lowercase name used on disk.</summary>
    public static bool TryParse(string? value, out AgentState state)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "idle": state = AgentState.Idle; return true;
            case "done": state = AgentState.Done; return true;
            case "working": state = AgentState.Working; return true;
            case "blocked": state = AgentState.Blocked; return true;
            case "error": state = AgentState.Error; return true;
            default: state = AgentState.Idle; return false;
        }
    }

    /// <summary>The word a person reads, not the one stored on disk.</summary>
    public static string Describe(this AgentState state) => state switch
    {
        AgentState.Error => "failed",
        AgentState.Blocked => "waiting",
        AgentState.Working => "working",
        AgentState.Done => "done",
        _ => "quiet",
    };
}
