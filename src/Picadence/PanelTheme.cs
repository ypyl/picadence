// The palette and the contrast maths. The painter reads these constants and so
// does the contrast test, so a colour that fails to read fails the build.
namespace Picadence;

/// <summary>A colour with no dependency on a drawing library.</summary>
public readonly record struct Rgb(byte Red, byte Green, byte Blue)
{
    /// <summary>The 0x00BBGGRR value GDI takes.</summary>
    public uint ToColorRef() => (uint)(Red | (Green << 8) | (Blue << 16));

    public static Rgb FromHex(string hex) => new(
        Convert.ToByte(hex[1..3], 16),
        Convert.ToByte(hex[3..5], 16),
        Convert.ToByte(hex[5..7], 16));
}

public static class PanelTheme
{
    public static readonly Rgb Surface = Rgb.FromHex("#202020");
    public static readonly Rgb Row = Rgb.FromHex("#2B2B2B");
    public static readonly Rgb Outline = Rgb.FromHex("#3A3A3A");
    public static readonly Rgb Hover = Rgb.FromHex("#3A3A3A");
    public static readonly Rgb PrimaryText = Rgb.FromHex("#FFFFFF");
    public static readonly Rgb SecondaryText = Rgb.FromHex("#C5C5C5");

    /// <summary>The smallest ratio WCAG asks for body text.</summary>
    public const double MinimumContrast = 4.5;

    /// <summary>The dot colour for a state. Never used behind text.</summary>
    public static Rgb State(AgentState state) => state switch
    {
        AgentState.Error => Rgb.FromHex("#DC2828"),
        AgentState.Blocked => Rgb.FromHex("#E6A000"),
        AgentState.Working => Rgb.FromHex("#28B450"),
        _ => Rgb.FromHex("#969696"),
    };

    /// <summary>
    /// WCAG 2.1 relative luminance and contrast. Computed here rather than
    /// estimated, so the number in the design is the number the test checks.
    /// </summary>
    public static double ContrastRatio(Rgb first, Rgb second)
    {
        var a = RelativeLuminance(first);
        var b = RelativeLuminance(second);
        var lighter = Math.Max(a, b);
        var darker = Math.Min(a, b);
        return (lighter + 0.05) / (darker + 0.05);
    }

    public static double RelativeLuminance(Rgb colour) =>
        (0.2126 * Channel(colour.Red)) + (0.7152 * Channel(colour.Green)) + (0.0722 * Channel(colour.Blue));

    private static double Channel(byte value)
    {
        var c = value / 255.0;
        return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
    }

    /// <summary>Every text colour against every surface it is drawn on.</summary>
    public static IEnumerable<(Rgb Text, Rgb Background)> Pairs()
    {
        yield return (PrimaryText, Surface);
        yield return (PrimaryText, Row);
        yield return (SecondaryText, Surface);
        yield return (SecondaryText, Row);
        yield return (PrimaryText, Hover);
    }
}
