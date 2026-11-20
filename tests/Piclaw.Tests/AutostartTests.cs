namespace Piclaw.Tests;

public class AutostartTests
{
    private const string Exe = @"C:\Users\ypyl\projects\piclaw\src\Piclaw\bin\Release\net10.0\win-x64\publish\Piclaw.exe";

    [Fact(DisplayName = "the registered path follows the app")]
    public void The_registered_path_follows_the_app()
    {
        var current = Autostart.CommandFor(Exe);

        Assert.True(Autostart.IsSetTo(current, Exe));
        Assert.False(Autostart.NeedsRepair(current, Exe));
        Assert.False(Autostart.IsSetTo(null, Exe));

        var moved = Autostart.CommandFor(@"D:\apps\piclaw\Piclaw.exe");
        Assert.True(Autostart.NeedsRepair(moved, Exe));
        Assert.Equal(Autostart.CommandFor(Exe), Autostart.CommandFor(Exe));
    }

    [Fact(DisplayName = "a path with spaces still starts")]
    public void A_path_with_spaces_still_starts()
    {
        var spaced = @"C:\Program Files\piclaw\Piclaw.exe";

        Assert.Equal('"' + spaced + '"', Autostart.CommandFor(spaced));
        Assert.True(Autostart.CommandFor(spaced).StartsWith('"'));
        Assert.True(Autostart.CommandFor(spaced).EndsWith('"'));
    }

    [Fact(DisplayName = "the autostart control turns it on and off")]
    public void The_autostart_control_turns_it_on_and_off()
    {
        var off = false;
        var on = Autostart.Toggle(off);
        Assert.True(on);

        var backOff = Autostart.Toggle(on);
        Assert.False(backOff);

        var offLayout = PanelModel.Build([], DateTimeOffset.UtcNow, off);
        var onLayout = PanelModel.Build([], DateTimeOffset.UtcNow, on);
        var row = offLayout.Actions.Single(action => action.Action == PanelAction.Autostart);

        Assert.Equal("Start with Windows", row.Label);
        Assert.Equal("off", row.Value);
        Assert.Equal("on", onLayout.Actions.Single(action => action.Action == PanelAction.Autostart).Value);
        Assert.Equal(PanelHit.Autostart, offLayout.HitTest(row.Bounds.X + 4, row.Bounds.Y + 4));
    }

    [Fact]
    public void A_blank_path_is_refused()
    {
        Assert.Throws<ArgumentException>(() => Autostart.CommandFor("  "));
    }
}
