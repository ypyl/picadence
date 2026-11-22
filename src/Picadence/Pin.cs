// A session the user asked the tray to keep. It is keyed by session id and holds
// the conversation's path, so it outlives the process that was running it, a tray
// restart and a reboot. Nothing here is derived from a process.
namespace Picadence;

public sealed record Pin(
    string SessionId,
    string? Name,
    string Folder,
    string? Transcript,
    DateTimeOffset KeptAt);
