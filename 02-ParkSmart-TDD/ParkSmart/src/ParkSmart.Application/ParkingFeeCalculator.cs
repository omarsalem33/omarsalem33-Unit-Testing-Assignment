using ParkSmart.Domain;

namespace ParkSmart.Application;

public sealed class ParkingFeeCalculator
{
    public decimal Calculate(ParkingSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (session.ExitTime <= session.EntryTime)
            throw new ArgumentException("Exit time must be after entry time.", nameof(session));
        if (session.ChargedKwh < 0)
            throw new ArgumentOutOfRangeException(nameof(session), "Charged kWh cannot be negative.");

        var minutes = (session.ExitTime - session.EntryTime).TotalMinutes;
        var normal = CalculateNormal(session, minutes);
        if (session.IsSubscriber)
        {
            var weekdayFree = session.EntryTime.Date == session.ExitTime.Date
                && session.EntryTime.DayOfWeek is >= DayOfWeek.Sunday and <= DayOfWeek.Thursday
                && session.EntryTime.TimeOfDay >= TimeSpan.FromHours(7)
                && session.ExitTime.TimeOfDay <= TimeSpan.FromHours(19);
            normal = weekdayFree ? 0m : Round(normal * 0.5m);
        }

        if (session.TicketLost)
            normal = Math.Max(normal, LostTicketMinimum(session.VehicleType));

        return Round(normal + session.ChargedKwh * 3.50m);
    }

    static decimal CalculateNormal(ParkingSession session, double minutes)
    {
        var graceMinutes = session.EntryTime.DayOfWeek == DayOfWeek.Friday ? 120d : 15d;
        if (minutes <= graceMinutes)
            return 0m;

        var rate = session.VehicleType switch
        {
            VehicleType.Motorcycle => 5m,
            VehicleType.Truck => 25m,
            _ => 10m
        };
        var cap = session.VehicleType switch
        {
            VehicleType.Motorcycle => 40m,
            VehicleType.Truck => 200m,
            _ => 80m
        };
        var remainingHours = Math.Ceiling(minutes / 60d);
        var total = 0m;
        while (remainingHours > 0)
        {
            var blockHours = Math.Min(remainingHours, 24d);
            total += Math.Min((int)blockHours * rate, cap);
            remainingHours -= blockHours;
        }

        if (session.EntryTime.Date == session.ExitTime.Date.AddDays(-1)
            && session.EntryTime.TimeOfDay >= TimeSpan.FromHours(22)
            && session.ExitTime.TimeOfDay <= TimeSpan.FromHours(8))
        {
            var flat = session.VehicleType switch
            {
                VehicleType.Motorcycle => 15m,
                VehicleType.Truck => 75m,
                _ => 30m
            };
            total = Math.Min(total, flat);
        }

        return total;
    }
    static decimal LostTicketMinimum(VehicleType type) => type switch
    {
        VehicleType.Motorcycle => 75m,
        VehicleType.Truck => 300m,
        _ => 150m
    };

    static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}







