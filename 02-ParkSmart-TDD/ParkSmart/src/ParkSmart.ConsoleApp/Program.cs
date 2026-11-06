using ParkSmart.Application;
using ParkSmart.Domain;

Console.WriteLine("ParkSmart parking fee calculator");
Console.Write("Parking duration in whole minutes: ");
if (!int.TryParse(Console.ReadLine(), out var minutes) || minutes < 1)
{
    Console.WriteLine("Enter a positive duration.");
    return;
}

var entry = new DateTime(2026, 1, 1, 9, 0, 0);
var session = new ParkingSession { EntryTime = entry, ExitTime = entry.AddMinutes(minutes) };
Console.WriteLine($"Fee: {new ParkingFeeCalculator().Calculate(session):0.00} EGP");
