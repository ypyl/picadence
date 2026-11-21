// How often a session is woken, and what a cadence does to a moment. Months and
// years move by the calendar, so a cadence armed on 31 January stays on the last
// day of February rather than sliding into March.
namespace Piclaw;

public enum Cadence
{
    /// <summary>Not armed.</summary>
    Off,

    Every15Minutes,
    Hourly,
    Weekly,
    Monthly,
    Yearly,
}

public static class CadenceExtensions
{
    /// <summary>
    /// How many windows can be reported as missed. A year of fifteen-minute
    /// windows is more than anyone wants to read, so the count stops and the
    /// message says "at least".
    /// </summary>
    // shortcut: the count stops at 1000 and the message then says "at least", count
    // exactly when someone needs the number for a longer gap than that
    public const int MaxMissed = 1000;

    /// <summary>The choices the panel offers, in the order it offers them.</summary>
    public static readonly Cadence[] Offered =
    [
        Cadence.Every15Minutes,
        Cadence.Hourly,
        Cadence.Weekly,
        Cadence.Monthly,
        Cadence.Yearly,
        Cadence.Off,
    ];

    /// <summary>Reads the key stored in a schedule file.</summary>
    public static bool TryParse(string? value, out Cadence cadence)
    {
        switch (value?.Trim())
        {
            case "15m": cadence = Cadence.Every15Minutes; return true;
            case "1h": cadence = Cadence.Hourly; return true;
            case "1w": cadence = Cadence.Weekly; return true;
            case "1mo": cadence = Cadence.Monthly; return true;
            case "1y": cadence = Cadence.Yearly; return true;
            case "off": cadence = Cadence.Off; return true;
            default: cadence = Cadence.Off; return false;
        }
    }

    /// <summary>The key stored in a schedule file.</summary>
    public static string Key(this Cadence cadence) => cadence switch
    {
        Cadence.Every15Minutes => "15m",
        Cadence.Hourly => "1h",
        Cadence.Weekly => "1w",
        Cadence.Monthly => "1mo",
        Cadence.Yearly => "1y",
        _ => "off",
    };

    /// <summary>The words a person reads, on the row and in the message.</summary>
    public static string Label(this Cadence cadence) => cadence switch
    {
        Cadence.Every15Minutes => "every 15 minutes",
        Cadence.Hourly => "every hour",
        Cadence.Weekly => "every week",
        Cadence.Monthly => "every month",
        Cadence.Yearly => "every year",
        _ => "off",
    };

    /// <summary>
    /// The moment one cadence after <paramref name="from"/>. Off does not move, so
    /// a caller that forgot to check gets a fixed point rather than a surprise.
    /// </summary>
    public static DateTimeOffset Advance(this Cadence cadence, DateTimeOffset from) => cadence switch
    {
        Cadence.Every15Minutes => from.AddMinutes(15),
        Cadence.Hourly => from.AddHours(1),
        Cadence.Weekly => from.AddDays(7),
        Cadence.Monthly => from.AddMonths(1),
        Cadence.Yearly => from.AddYears(1),
        _ => from,
    };

    /// <summary>
    /// How many windows fell due after <paramref name="nextAt"/> and before
    /// <paramref name="now"/>. The window about to be served does not count, so a
    /// session that is exactly on time reports nothing missed.
    /// </summary>
    public static int MissedBetween(this Cadence cadence, DateTimeOffset nextAt, DateTimeOffset now)
    {
        if (cadence == Cadence.Off || now < nextAt)
        {
            return 0;
        }

        var missed = 0;
        var at = cadence.Advance(nextAt);
        while (at < now && missed < MaxMissed)
        {
            missed++;
            at = cadence.Advance(at);
        }

        return missed;
    }
}
