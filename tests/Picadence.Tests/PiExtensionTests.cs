namespace Picadence.Tests;

public class PiExtensionTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(), "picadence-pihalf-" + Guid.NewGuid().ToString("N"));

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

    private string Settings(string relative, string packages)
    {
        var path = Path.Combine(_root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "{\"packages\":[" + packages + "]}");
        return path;
    }

    [Fact(DisplayName = "a settings file that lists picadence reads as declared")]
    public void A_settings_file_that_lists_picadence_reads_as_declared()
    {
        var file = Settings(
            "agent/settings.json",
            "\"git:github.com/DietrichGebert/ponytail\",\"git:github.com/ypyl/picadence\"");

        Assert.True(PiExtension.Declares(file));
        Assert.Equal(PiHalf.Declared, PiExtension.Check(anythingReported: false, [file]));
        Assert.False(PiExtension.Declares(Path.Combine(_root, "nowhere.json")));
    }

    [Fact(DisplayName = "a settings file that does not, reads as missing")]
    public void A_settings_file_that_does_not_reads_as_missing()
    {
        var file = Settings("agent/settings.json", "\"git:github.com/DietrichGebert/ponytail\"");

        Assert.False(PiExtension.Declares(file));
        Assert.Equal(PiHalf.Missing, PiExtension.Check(anythingReported: false, [file]));

        // A file that is not JSON, or has no packages, says nothing rather than guessing.
        File.WriteAllText(file, "not json at all");
        Assert.False(PiExtension.Declares(file));
        File.WriteAllText(file, "{\"theme\":\"dark\"}");
        Assert.False(PiExtension.Declares(file));
    }

    [Fact(DisplayName = "a project-local file counts as well as the user one")]
    public void A_project_local_file_counts_as_well_as_the_user_one()
    {
        var project = Path.Combine(_root, "project");
        Settings("project/.pi/settings.json", "\"git:github.com/ypyl/picadence\"");
        var user = Settings("agent/settings.json", "\"git:github.com/DietrichGebert/ponytail\"");

        var files = PiExtension.SettingsFiles([project]).ToList();

        // The user's own file comes first, then one per folder, so a project-local
        // install is found even when the user file never heard of it.
        Assert.Equal(2, files.Count);
        Assert.EndsWith(Path.Combine(".pi", "agent", "settings.json"), files[0], StringComparison.OrdinalIgnoreCase);
        Assert.Equal(Path.Combine(project, ".pi", "settings.json"), files[1]);
        Assert.True(PiExtension.Declares(user) is false);
        Assert.True(PiExtension.Declares(files[1]));
        Assert.Equal(PiHalf.Declared, PiExtension.Check(anythingReported: false, files));
    }

    [Fact(DisplayName = "anything reported outranks both")]
    public void Anything_reported_outranks_both()
    {
        // A live session is proof the extension runs, whatever the settings say.
        Assert.Equal(PiHalf.Working, PiExtension.Check(anythingReported: true, []));
        Assert.Equal(PiHalf.Working, PiExtension.Check(anythingReported: true, [_root]));
    }
}
