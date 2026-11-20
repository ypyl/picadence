using System.Reflection;
using System.Runtime.InteropServices;

namespace Piclaw.Tests;

public class IconPixelsTests
{
    [Fact]
    public void Each_urgent_state_has_its_own_colour()
    {
        var urgent = new[]
        {
            IconPixels.Color(AgentState.Error),
            IconPixels.Color(AgentState.Blocked),
            IconPixels.Color(AgentState.Working),
        };

        Assert.Equal(3, urgent.Distinct().Count());
    }

    [Fact]
    public void A_finished_and_a_quiet_session_share_the_grey()
    {
        Assert.Equal(IconPixels.Color(AgentState.Done), IconPixels.Color(AgentState.Idle));
        Assert.DoesNotContain(IconPixels.Color(AgentState.Idle), new[]
        {
            IconPixels.Color(AgentState.Error),
            IconPixels.Color(AgentState.Blocked),
            IconPixels.Color(AgentState.Working),
        });
    }

    [Fact]
    public void The_dot_is_a_circle_with_a_transparent_corner()
    {
        var pixels = IconPixels.Bgra(AgentState.Working);

        Assert.Equal(IconPixels.Size * IconPixels.Size * 4, pixels.Length);
        Assert.Equal(0xFF, IconPixels.AlphaAt(pixels, 8, 8));
        Assert.Equal(0x00, IconPixels.AlphaAt(pixels, 0, 0));
        Assert.Equal(0x00, IconPixels.AlphaAt(pixels, IconPixels.Size - 1, IconPixels.Size - 1));
    }

    [Fact]
    public void The_mask_is_the_size_windows_expects_and_lets_alpha_decide()
    {
        var mask = IconPixels.AndMask();

        Assert.Equal(IconPixels.Size * 4, mask.Length);
        Assert.All(mask, value => Assert.Equal(0, value));
    }
}

public class InteropTests
{
    private static readonly string[] Allowed =
    [
        "user32.dll",
        "kernel32.dll",
        "shell32.dll",
        "gdi32.dll",
        "dwmapi.dll",
    ];

    [Fact(DisplayName = "the tray links no network entry point")]
    public void The_tray_links_no_network_entry_point()
    {
        var libraries = typeof(Native)
            .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Select(method => method.GetCustomAttribute<DllImportAttribute>()?.Value)
            .Where(value => value is not null)
            .Select(value => value!)
            .Distinct()
            .ToList();

        Assert.NotEmpty(libraries);
        Assert.All(libraries, library => Assert.Contains(library, Allowed, StringComparer.OrdinalIgnoreCase));
    }
}
