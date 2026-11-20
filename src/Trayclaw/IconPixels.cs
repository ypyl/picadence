// The icon, as data. Sixteen pixels square, painted with one colour per state,
// and turned into a Windows icon by Program.cs. Keeping the pixels pure means the
// colour, which is the whole point of a glance, is a test and not a guess.
namespace Trayclaw;

public static class IconPixels
{
    public const int Size = 16;

    private const int Centre = 8;
    private const double Radius = 6.5;

    /// <summary>Red for a failure, amber for a question, green for work, grey for quiet.</summary>
    public static (byte Red, byte Green, byte Blue) Color(AgentState state) => state switch
    {
        AgentState.Error => (0xDC, 0x28, 0x28),
        AgentState.Blocked => (0xE6, 0xA0, 0x00),
        AgentState.Working => (0x28, 0xB4, 0x50),
        _ => (0x96, 0x96, 0x96),
    };

    /// <summary>Straight BGRA with alpha, which is what CreateIcon takes.</summary>
    public static byte[] Bgra(AgentState state)
    {
        var (red, green, blue) = Color(state);
        var pixels = new byte[Size * Size * 4];

        for (var y = 0; y < Size; y++)
        {
            for (var x = 0; x < Size; x++)
            {
                var dx = x + 0.5 - Centre;
                var dy = y + 0.5 - Centre;
                var inside = (dx * dx) + (dy * dy) <= Radius * Radius;

                var offset = ((y * Size) + x) * 4;
                pixels[offset] = inside ? blue : (byte)0;
                pixels[offset + 1] = inside ? green : (byte)0;
                pixels[offset + 2] = inside ? red : (byte)0;
                pixels[offset + 3] = inside ? (byte)0xFF : (byte)0x00;
            }
        }

        return pixels;
    }

    /// <summary>
    /// The monochrome mask, all zero, so the shape comes from the alpha channel
    /// alone. Each row is padded to four bytes.
    /// </summary>
    public static byte[] AndMask() => new byte[Size * 4];

    /// <summary>Alpha at one pixel, which is what transparency means here.</summary>
    public static byte AlphaAt(byte[] pixels, int x, int y) => pixels[((y * Size) + x) * 4 + 3];
}
