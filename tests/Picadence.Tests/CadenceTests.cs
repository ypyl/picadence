namespace Picadence.Tests;

public class CadenceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 21, 14, 3, 0, TimeSpan.Zero);

    [Fact(DisplayName = "a monthly cadence keeps its calendar day")]
    public void A_monthly_cadence_keeps_its_calendar_day()
    {
        var january = new DateTimeOffset(2026, 1, 31, 9, 0, 0, TimeSpan.Zero);
        Assert.Equal(
            new DateTimeOffset(2026, 2, 28, 9, 0, 0, TimeSpan.Zero),
            Cadence.Monthly.Advance(january));

        var leap = new DateTimeOffset(2028, 1, 31, 9, 0, 0, TimeSpan.Zero);
        Assert.Equal(
            new DateTimeOffset(2028, 2, 29, 9, 0, 0, TimeSpan.Zero),
            Cadence.Monthly.Advance(leap));

        var march = new DateTimeOffset(2026, 3, 31, 9, 0, 0, TimeSpan.Zero);
        Assert.Equal(
            new DateTimeOffset(2026, 4, 30, 9, 0, 0, TimeSpan.Zero),
            Cadence.Monthly.Advance(march));
    }

    [Theory]
    [InlineData(Cadence.Every15Minutes, "15m", "every 15 minutes", 15)]
    [InlineData(Cadence.Hourly, "1h", "every hour", 60)]
    [InlineData(Cadence.Weekly, "1w", "every week", 7 * 24 * 60)]
    public void A_duration_cadence_advances_by_its_length(
        Cadence cadence, string key, string label, int minutes)
    {
        Assert.Equal(key, cadence.Key());
        Assert.Equal(label, cadence.Label());
        Assert.Equal(Now.AddMinutes(minutes), cadence.Advance(Now));
        Assert.True(CadenceExtensions.TryParse(key, out var parsed));
        Assert.Equal(cadence, parsed);
    }

    [Fact]
    public void The_off_cadence_moves_nothing_and_reads_as_words()
    {
        Assert.Equal("off", Cadence.Off.Key());
        Assert.Equal("off", Cadence.Off.Label());
        Assert.Equal(Now, Cadence.Off.Advance(Now));
        Assert.True(CadenceExtensions.TryParse("off", out var parsed));
        Assert.Equal(Cadence.Off, parsed);
        Assert.False(CadenceExtensions.TryParse("2h", out _));
        Assert.False(CadenceExtensions.TryParse(null, out _));
    }

    [Fact]
    public void The_flyout_offers_five_cadences_and_off()
    {
        Assert.Equal(6, CadenceExtensions.Offered.Length);
        Assert.Equal(Cadence.Off, CadenceExtensions.Offered[^1]);
    }

    [Fact]
    public void The_window_about_to_be_served_does_not_count_as_missed()
    {
        Assert.Equal(0, Cadence.Hourly.MissedBetween(Now, Now));
        Assert.Equal(0, Cadence.Hourly.MissedBetween(Now, Now.AddMinutes(-1)));
        Assert.Equal(23, Cadence.Hourly.MissedBetween(Now.AddDays(-1), Now));
        Assert.Equal(
            CadenceExtensions.MaxMissed,
            Cadence.Every15Minutes.MissedBetween(Now.AddYears(-10), Now));
    }
}
