namespace ParkSmart.Domain;

public enum VehicleType { Car, Motorcycle, Truck }

public sealed class ParkingSession
{
    public DateTime EntryTime { get; set; }
    public DateTime ExitTime { get; set; }
    public VehicleType VehicleType { get; set; } = VehicleType.Car;
    public bool IsSubscriber { get; set; }
    public bool TicketLost { get; set; }
    public decimal ChargedKwh { get; set; }
}
