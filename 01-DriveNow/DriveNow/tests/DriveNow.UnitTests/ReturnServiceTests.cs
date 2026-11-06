using DriveNow.Application.Bookings;
using DriveNow.Application.Services;
using FluentAssertions;

namespace DriveNow.UnitTests;

public class ReturnServiceTests
{
    [Theory]
    [Trait("Rule", "BR-41")]
    [InlineData(0, 0)] [InlineData(59, 0)] [InlineData(60, 60)] [InlineData(61, 120)] [InlineData(240, 240)] [InlineData(241, 600)]
    public void LateFee_applies_the_correct_boundary(int minutesLate, decimal expected)
    {
        ReturnService.LateFee(minutesLate, 600m).Should().Be(expected);
    }
}

public class CancellationServiceTests
{
    [Theory]
    [Trait("Rule", "BR-51")]
    [InlineData(48, false, 0)] [InlineData(24, false, 300)] [InlineData(23.99, false, 600)] [InlineData(1, true, 0)]
    public void CalculateCancellationFee_uses_hour_boundary_and_platinum_exemption(double hours, bool platinum, decimal expected)
    {
        CancellationService.CalculateCancellationFee(hours, 600m, platinum).Should().Be(expected);
    }
}
