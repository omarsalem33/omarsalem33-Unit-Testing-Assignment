using ParkSmart.Application;
using ParkSmart.Domain;
using FluentAssertions;

namespace ParkSmart.UnitTests;

public class ParkingFeeCalculatorTests
{
    readonly ParkingFeeCalculator _sut = new();
    static DateTime Entry(int hour = 9, int day = 5) => new(2026, 11, day, hour, 0, 0);
    static ParkingSession Session(int minutes, VehicleType type = VehicleType.Car, int hour = 9, int day = 5) => new()
    {
        EntryTime = Entry(hour, day), ExitTime = Entry(hour, day).AddMinutes(minutes), VehicleType = type
    };

    [Theory]
    [InlineData(15, 0)] [InlineData(16, 10)] [InlineData(60, 10)] [InlineData(61, 20)] [InlineData(180, 30)]
    public void Calculate_applies_grace_and_started_hours(int minutes, decimal expected) =>
        _sut.Calculate(Session(minutes)).Should().Be(expected);

[Fact]
    public void Calculate_rejects_exit_at_or_before_entry()
    {
        Action act = () => _sut.Calculate(new ParkingSession { EntryTime = Entry(), ExitTime = Entry() });
        act.Should().Throw<ArgumentException>().WithMessage("Exit time must be after entry time.*");
    }

    [Theory]
    [InlineData(600, 80)] [InlineData(1800, 140)] [InlineData(2890, 170)]
    public void Calculate_caps_each_24_hour_block(int minutes, decimal expected) =>
        _sut.Calculate(Session(minutes, day: 2)).Should().Be(expected);

    [Theory]
    [InlineData(VehicleType.Motorcycle, 180, 15)] [InlineData(VehicleType.Truck, 600, 200)] [InlineData(VehicleType.Truck, 1800, 350)]
    public void Calculate_uses_vehicle_rate_and_cap(VehicleType type, int minutes, decimal expected) =>
        _sut.Calculate(Session(minutes, type, day: 2)).Should().Be(expected);

    [Theory]
    [InlineData(22, 60, 10)] [InlineData(23, 480, 30)]
    public void Calculate_applies_night_flat_fee(int hour, int minutes, decimal expected)
    {
        var entry = new DateTime(2026, 11, hour == 23 ? 2 : 2, hour, 0, 0);
        var exit = hour == 23 ? entry.Date.AddDays(1).AddHours(7) : entry.AddMinutes(minutes);
        _sut.Calculate(new ParkingSession { EntryTime = entry, ExitTime = exit }).Should().Be(expected);
    }

    [Theory]
    [InlineData(120, 0)] [InlineData(121, 30)]
    public void Calculate_uses_longer_friday_grace(int minutes, decimal expected) =>
        _sut.Calculate(Session(minutes, day: 6)).Should().Be(expected);

    [Fact]
    public void Calculate_applies_subscriber_half_discount()
    {
        var session = Session(180, day: 7); session.IsSubscriber = true;
        _sut.Calculate(session).Should().Be(15m);
    }

    [Fact]
    public void Calculate_makes_weekday_business_window_free()
    {
        var entry = new DateTime(2026, 11, 2, 9, 0, 0); // Monday
        _sut.Calculate(new ParkingSession { EntryTime = entry, ExitTime = entry.AddHours(8), IsSubscriber = true }).Should().Be(0m);
    }

    [Theory]
    [InlineData(VehicleType.Car, 150)] [InlineData(VehicleType.Motorcycle, 75)] [InlineData(VehicleType.Truck, 300)]
    public void Calculate_applies_lost_ticket_minimum(VehicleType type, decimal expected)
    {
        var session = Session(15, type); session.TicketLost = true;
        _sut.Calculate(session).Should().Be(expected);
    }

    [Fact]
    public void Calculate_adds_ev_charge_after_discounts()
    {
        var session = Session(120, day: 2); session.IsSubscriber = true; session.ChargedKwh = 4;
        _sut.Calculate(session).Should().Be(14m);
    }

    [Fact]
    public void Calculate_rejects_negative_energy()
    {
        Action act = () => { var session = Session(60); session.ChargedKwh = -1; _sut.Calculate(session); };
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}


