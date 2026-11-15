using DriveNow.Domain.Abstractions;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;

namespace DriveNow.Application.Bookings;

public class CancellationService
{
    readonly IBookingRepository _bookings; readonly IVehicleRepository _vehicles; readonly ICustomerRepository _customers;
    readonly IPromoCodeRepository _promos; readonly IPaymentGateway _payments; readonly ISmsSender _sms; readonly IEmailSender _email;
    readonly IAuditLogger _audit; readonly IClock _clock;
    public CancellationService(IBookingRepository bookings, IVehicleRepository vehicles, ICustomerRepository customers,
        IPromoCodeRepository promos, IPaymentGateway payments, ISmsSender sms, IEmailSender email, IAuditLogger audit, IClock clock)
    { _bookings=bookings; _vehicles=vehicles; _customers=customers; _promos=promos; _payments=payments; _sms=sms; _email=email; _audit=audit; _clock=clock; }

    public decimal Cancel(int bookingId)
    {
        var booking = _bookings.GetById(bookingId) ?? throw new BookingNotFoundException($"Booking #{bookingId} not found");
        if (booking.Status != BookingStatus.Reserved) throw new InvalidBookingStateException("Only reserved bookings can be cancelled");
        var now = _clock.Now;
        if (now >= booking.PickupAt) throw new InvalidBookingStateException("Booking has already started");
        var vehicle = _vehicles.GetById(booking.VehicleId) ?? throw new VehicleNotFoundException($"Vehicle #{booking.VehicleId} not found");
        var customer = _customers.GetById(booking.CustomerId) ?? throw new CustomerNotFoundException($"Customer #{booking.CustomerId} not found");
        var fee = CalculateCancellationFee((booking.PickupAt - now).TotalHours, vehicle.DailyRate, customer.LoyaltyTier == LoyaltyTier.Platinum);
        var refund = Math.Max(booking.Deposit - fee, 0m);
        if (refund > 0) _payments.Refund(customer.Id, refund, $"Deposit refund {booking.Reference}");
        if (fee > booking.Deposit) _payments.Charge(customer.Id, fee - booking.Deposit, $"Cancellation {booking.Reference}");
        booking.Status = BookingStatus.Cancelled; _bookings.Update(booking);
        if (!string.IsNullOrWhiteSpace(booking.PromoCode))
        {
            var promo = _promos.GetByCode(booking.PromoCode);
            if (promo is not null) { promo.UsedCount--; _promos.Update(promo); }
        }
        var note = $"Booking {booking.Reference} cancelled"; _sms.Send(customer.Phone, note); _email.Send(customer.Email, note, note); _audit.Write(note);
        return fee;
    }

    internal static decimal CalculateCancellationFee(double hoursUntilPickup, decimal dailyRate, bool platinum)
    {
        if (platinum || hoursUntilPickup >= 48) return 0m;
        return Math.Round(hoursUntilPickup >= 24 ? dailyRate * 0.50m : dailyRate, 2, MidpointRounding.AwayFromZero);
    }
}
