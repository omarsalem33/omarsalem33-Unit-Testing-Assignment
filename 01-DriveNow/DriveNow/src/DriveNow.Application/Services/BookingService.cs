using DriveNow.Domain.Abstractions;
using DriveNow.Application.Abstractions;
using DriveNow.Application.Models;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;
using DriveNow.Domain.Services;

namespace DriveNow.Application.Services;

public class BookingService
{
    readonly ICustomerRepository _customers;
    readonly IVehicleRepository _vehicles;
    readonly IBookingRepository _bookings;
    readonly IPromoCodeRepository _promos;
    readonly IPricingService _pricing;
    readonly IPaymentGateway _payments;
    readonly ISmsSender _sms;
    readonly IEmailSender _email;
    readonly IAuditLogger _audit;
    readonly IClock _clock;
    readonly IBookingReferenceGenerator _references;

    public BookingService(ICustomerRepository customers, IVehicleRepository vehicles, IBookingRepository bookings,
        IPromoCodeRepository promos, IPricingService pricing, IPaymentGateway payments, ISmsSender sms,
        IEmailSender email, IAuditLogger audit, IClock clock, IBookingReferenceGenerator references)
    {
        _customers = customers; _vehicles = vehicles; _bookings = bookings; _promos = promos; _pricing = pricing;
        _payments = payments; _sms = sms; _email = email; _audit = audit; _clock = clock; _references = references;
    }

    public Booking CreateBooking(BookingRequest request)
    {
        var customer = _customers.GetById(request.CustomerId)
            ?? throw new CustomerNotFoundException($"Customer #{request.CustomerId} not found");
        var age = RentalMath.CalculateAge(customer.DateOfBirth, request.PickupAt);
        var licenseYears = RentalMath.CalculateLicenseYears(customer.LicenseIssueDate, request.PickupAt);
        RejectCustomer(customer, age, licenseYears);

        var vehicle = _vehicles.GetById(request.VehicleId)
            ?? throw new VehicleNotFoundException($"Vehicle #{request.VehicleId} not found");
        RejectVehicle(vehicle, age, licenseYears, request, _clock.Now);
        if (request.AdditionalDrivers is < 0 or > 3)
            throw new BookingRejectedException("Up to 3 additional drivers allowed");
        if (_bookings.GetOpenBookingsForVehicle(vehicle.Id).Any(existing =>
                request.PickupAt < existing.ReturnAt && request.ReturnAt > existing.PickupAt))
            throw new BookingRejectedException("Vehicle is already booked for these dates");
        if (_bookings.CountOpenBookingsForCustomer(customer.Id) >= 2)
            throw new BookingRejectedException("Customer already has 2 active bookings");

        var price = _pricing.Calculate(customer, vehicle, request);
        var booking = new Booking
        {
            CustomerId = customer.Id, VehicleId = vehicle.Id, PickupAt = request.PickupAt, ReturnAt = request.ReturnAt,
            InsurancePlan = request.InsurancePlan, AdditionalDrivers = request.AdditionalDrivers, PromoCode = request.PromoCode,
            Price = price, Status = BookingStatus.Reserved, CreatedAt = _clock.Now, StartMileage = vehicle.Mileage,
            Reference = _references.Generate(request.PickupAt)
        };
        booking.Deposit = DepositFor(vehicle.Category, request.InsurancePlan, customer.LoyaltyTier);
        if (booking.Deposit > 0) _payments.Charge(customer.Id, booking.Deposit, $"Deposit {booking.Reference}");

        if (!string.IsNullOrWhiteSpace(request.PromoCode))
        {
            var promo = _promos.GetByCode(request.PromoCode);
            if (promo is not null) { promo.UsedCount++; _promos.Update(promo); }
        }
        _bookings.Add(booking);
        var note = $"Booking {booking.Reference} confirmed";
        _sms.Send(customer.Phone, note);
        _email.Send(customer.Email, note, note);
        _audit.Write($"Booking {booking.Reference} created");
        return booking;
    }

    public void StartRental(int bookingId)
    {
        var booking = _bookings.GetById(bookingId)
            ?? throw new BookingNotFoundException($"Booking #{bookingId} not found");
        if (booking.Status != BookingStatus.Reserved)
            throw new InvalidBookingStateException("Only reserved bookings can be started");
        if (_clock.Now < booking.PickupAt.AddHours(-1))
            throw new InvalidBookingStateException("Too early to start this rental");
        booking.Status = BookingStatus.Active;
        _bookings.Update(booking);
        _audit.Write($"Booking {booking.Reference} started");
    }

    static void RejectCustomer(Customer customer, int age, int licenseYears)
    {
        if (customer.IsBlacklisted) throw new BookingRejectedException("Customer is blacklisted");
        if (age < 21) throw new BookingRejectedException("Driver must be at least 21");
        if (licenseYears < 1) throw new BookingRejectedException("License must be held for at least 1 year");
    }

    static void RejectVehicle(Vehicle vehicle, int age, int licenseYears, BookingRequest request, DateTime now)
    {
        if (!vehicle.IsAvailable) throw new BookingRejectedException("Vehicle is not available");
        if (vehicle.Category == VehicleCategory.Luxury && !(age >= 25 && licenseYears >= 3))
            throw new BookingRejectedException("Luxury vehicles require age 25+ and 3 years of license");
        if (vehicle.Category == VehicleCategory.Van && licenseYears < 2)
            throw new BookingRejectedException("Vans require 2 years of license");
        if (request.PickupAt < now.AddHours(2)) throw new BookingRejectedException("Pickup must be at least 2 hours from now");
        if (request.ReturnAt <= request.PickupAt || request.ReturnAt - request.PickupAt > TimeSpan.FromDays(30))
            throw new BookingRejectedException("Rental period is invalid");
    }

    static decimal DepositFor(VehicleCategory category, InsurancePlan insurance, LoyaltyTier tier)
    {
        if (tier == LoyaltyTier.Platinum) return 0m;
        var deposit = category switch
        {
            VehicleCategory.Economy or VehicleCategory.Compact => 1000m,
            VehicleCategory.SUV or VehicleCategory.Van => 2500m,
            VehicleCategory.Luxury => 5000m,
            _ => 0m
        };
        return insurance == InsurancePlan.Premium ? Math.Round(deposit / 2m, 2, MidpointRounding.AwayFromZero) : deposit;
    }
}


