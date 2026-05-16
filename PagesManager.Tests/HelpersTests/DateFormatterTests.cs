using FluentAssertions;
using PagesManager.Core.Helpers;

namespace PagesManager.Tests.HelpersTests;

public class DateFormatterTests
{
    [Fact]
    public void FormatRelative_WhenDateIsToday_ShouldReturnTime()
    {
        var now = new DateTime(2026, 5, 14, 15, 30, 0, DateTimeKind.Utc);
        var date = new DateTime(2026, 5, 14, 12, 10, 0, DateTimeKind.Utc);

        var result = DateFormatter.FormatRelative(date, now);

        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain(":");
    }

    [Fact]
    public void FormatRelative_WhenDateIsYesterday_ShouldReturnYesterday()
    {
        var now = new DateTime(2026, 5, 14, 15, 30, 0, DateTimeKind.Utc);
        var date = now.AddDays(-1);

        var result = DateFormatter.FormatRelative(date, now);

        result.Should().Be("Вчера");
    }

    [Fact]
    public void FormatRelative_WhenDateIsOlderThanYear_ShouldReturnFullDate()
    {
        var now = new DateTime(2026, 5, 14, 15, 30, 0, DateTimeKind.Utc);
        var date = new DateTime(2024, 5, 12, 10, 0, 0, DateTimeKind.Utc);

        var result = DateFormatter.FormatRelative(date, now);

        result.Should().Contain("2024");
    }
}