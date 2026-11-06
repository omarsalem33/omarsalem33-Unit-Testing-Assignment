using DriveNow.Domain.Entities;
namespace DriveNow.Domain.Abstractions;
public interface IClock { DateTime Now { get; } DateTime Today { get; } }
public interface ICustomerRepository { Customer? GetById(int id); void Update(Customer customer); }
public interface IVehicleRepository { Vehicle? GetById(int id); void Update(Vehicle vehicle); }
public interface IBookingRepository { Booking? GetById(int id); void Add(Booking booking); void Update(Booking booking); List<Booking> GetOpenBookingsForVehicle(int vehicleId); int CountOpenBookingsForCustomer(int customerId); }
public interface IPromoCodeRepository { PromoCode? GetByCode(string code); void Update(PromoCode promo); }
public interface IPaymentGateway { string Charge(int customerId, decimal amount, string description); void Refund(int customerId, decimal amount, string description); }
public interface ISmsSender { void Send(string phone, string text); }
public interface IEmailSender { void Send(string to, string subject, string body); }
public interface IAuditLogger { void Write(string message); }
public interface IBookingReferenceGenerator { string Generate(DateTime pickupAt); }




