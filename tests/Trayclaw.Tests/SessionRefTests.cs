namespace Trayclaw.Tests;

public class SessionRefTests
{
    private static readonly string StateDirectory = Path.Combine(Path.GetTempPath(), "trayclaw-state");

    [Fact]
    public void FileName_ForAnOrdinarySession_IsPidThenSession()
    {
        Assert.True(SessionRef.TryFileName(1234, "01a1fa93-ff0c-71eb-afd8-f7959610ae72", out var name));
        Assert.Equal("1234-01a1fa93-ff0c-71eb-afd8-f7959610ae72.json", name);
    }

    [Theory]
    [InlineData("../../evil")]
    [InlineData("a/b")]
    [InlineData(@"..\..\evil")]
    [InlineData("a:b")]
    [InlineData("a b")]
    [InlineData("a.b")]
    [InlineData("a\u0000b")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("..")]
    public void FileName_RejectsAnythingThatIsNotAPlainFileStem(string? sessionId)
    {
        Assert.False(SessionRef.TryFileName(1234, sessionId, out var name));
        Assert.Empty(name);
    }

    [Fact]
    public void FileName_RejectsANonPositiveProcessId()
    {
        Assert.False(SessionRef.TryFileName(0, "abcd", out _));
        Assert.False(SessionRef.TryFileName(-1, "abcd", out _));
    }

    [Fact]
    public void FileName_KeepsAnAcceptedNameInsideTheStateDirectory()
    {
        Assert.True(SessionRef.TryFileName(1234, "abcd-5678", out var name));
        var resolved = Path.GetFullPath(Path.Combine(StateDirectory, name));
        var root = Path.GetFullPath(StateDirectory) + Path.DirectorySeparatorChar;

        Assert.StartsWith(root, resolved, StringComparison.Ordinal);
    }

    [Fact]
    public void TryParse_ReadsBackWhatFileNameWrote()
    {
        Assert.True(SessionRef.TryFileName(4321, "session-01", out var name));
        Assert.True(SessionRef.TryParse(name, out var parsed));
        Assert.Equal(new SessionRef(4321, "session-01"), parsed);
    }

    [Theory]
    [InlineData("../1234-a.json")]
    [InlineData("1234-a/../../x.json")]
    [InlineData("1234-a.json.exe")]
    [InlineData("notanumber-a.json")]
    [InlineData("0-a.json")]
    [InlineData("-1234-a.json")]
    [InlineData("1234-.json")]
    [InlineData("1234-a:b.json")]
    [InlineData("1234-a b.json")]
    [InlineData("a.json")]
    [InlineData("")]
    [InlineData(null)]
    public void TryParse_RejectsAnythingFileNameWouldNotProduce(string? fileName)
    {
        Assert.False(SessionRef.TryParse(fileName, out _));
    }
}
