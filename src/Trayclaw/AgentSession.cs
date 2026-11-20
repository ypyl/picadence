namespace Trayclaw;

/// <summary>One live agent session, as the tray sees it.</summary>
/// <param name="Reference">The process and session the state belongs to.</param>
/// <param name="Folder">The folder name of the session's working directory.</param>
/// <param name="Name">The session name, when one was set.</param>
/// <param name="Model">The model the session runs, when known.</param>
/// <param name="State">What the session is doing now.</param>
/// <param name="Detail">A short label: the tool, or the question being asked.</param>
/// <param name="UpdatedAt">When the session last changed state.</param>
public sealed record AgentSession(
    SessionRef Reference,
    string Folder,
    string? Name,
    string? Model,
    AgentState State,
    string? Detail,
    DateTimeOffset UpdatedAt)
{
    public int Pid => Reference.Pid;
}
