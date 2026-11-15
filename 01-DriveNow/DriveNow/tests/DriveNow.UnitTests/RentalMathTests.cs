using DriveNow.Domain.Services;
using FluentAssertions;

namespace DriveNow.UnitTests;

public class RentalMathTests
{
    [Theory]
    [Trait("Rule", "BR-01")]
    [InlineData("2000-06-15", "2025-06-14", 24)]
    [InlineData("2000-06-15", "2025-06-15", 25)]
    [InlineData("2000-06-15", "2025-06-16", 25)]
    public void CalculateAge_counts_only_full_years(string birth, string onDate, int expected)
    {
        var actual = RentalMath.CalculateAge(DateTime.Parse(birth), DateTime.Parse(onDate));
        actual.Should().Be(expected);
    }

    [Theory]
    [Trait("Rule", "BR-01")]
    [InlineData("2000-02-29", "2025-02-27", 24)]
    [InlineData("2000-02-29", "2025-02-28", 25)]
    public void CalculateAge_uses_February_28_for_leap_day_birthdays_in_non_leap_years(string birth, string onDate, int expected)
    {
        var actual = RentalMath.CalculateAge(DateTime.Parse(birth), DateTime.Parse(onDate));
        actual.Should().Be(expected);
    }

    [Theory]
    [Trait("Rule", "BR-02")]
    [InlineData("2020-04-10", "2021-04-09", 0)]
    [InlineData("2020-04-10", "2021-04-10", 1)]
    public void CalculateLicenseYears_counts_full_years(string issued, string onDate, int expected)
    {
        var actual = RentalMath.CalculateLicenseYears(DateTime.Parse(issued), DateTime.Parse(onDate));
        actual.Should().Be(expected);
    }

    [Theory]
    [Trait("Rule", "BR-03")]
    [InlineData(24, 59, 1)]
    [InlineData(25, 0, 1)]
    [InlineData(25, 1, 2)]
    [InlineData(3, 0, 1)]
    [InlineData(72, 0, 3)]
    public void CalculateRentalDays_applies_more_than_sixty_minute_grace(int hours, int minutes, int expected)
    {
        var pickup = new DateTime(2026, 1, 1);
        var actual = RentalMath.CalculateRentalDays(pickup, pickup.AddHours(hours).AddMinutes(minutes));
        actual.Should().Be(expected);
    }

    [Fact]
    [Trait("Rule", "BR-03")]
    public void CalculateRentalDays_rejects_return_at_or_before_pickup()
    {
        var pickup = new DateTime(2026, 1, 1);
        var act = () => RentalMath.CalculateRentalDays(pickup, pickup);
        act.Should().Throw<ArgumentException>().WithMessage("Return must be after pickup");
    }
}
