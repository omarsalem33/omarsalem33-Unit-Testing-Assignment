using AutoFixture.Xunit2;
using DriveNow.Application.Abstractions;
using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Domain.Abstractions;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;
using DriveNow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace DriveNow.UnitTests;

public sealed class BookingServiceTests
{
    static readonly DateTime Now = new(2026, 11, 5, 8, 0, 0);

    [Fact, Trait("Rule", "BR-10")]
    public void CreateBooking_rejects_missing_customer_before_other_checks()
    {
        var f = new Fixture();
        var act = () => f.Sut.CreateBooking(f.Request(customerId: 404, vehicleId: 404));
        act.Should().Throw<CustomerNotFoundException>().WithMessage("Customer #404 not found");
    }

    [Fact, Trait("Rule", "BR-11")]
    public void CreateBooking_rejects_blacklisted_customer()
    {
        var f = new Fixture(); f.Customer.IsBlacklisted = true;
        var act = () => f.Sut.CreateBooking(f.Request());
        act.Should().Throw<BookingRejectedException>().WithMessage("Customer is blacklisted");
    }

    [Fact, Trait("Rule", "BR-12")]
    public void CreateBooking_rejects_driver_under_21()
    {
        var f = new Fixture(); f.Customer.DateOfBirth = Now.AddYears(-20);
        var act = () => f.Sut.CreateBooking(f.Request());
        act.Should().Throw<BookingRejectedException>().WithMessage("Driver must be at least 21");
    }

    [Fact, Trait("Rule", "BR-13")]
    public void CreateBooking_rejects_license_held_less_than_one_year()
    {
        var f = new Fixture(); f.Customer.LicenseIssueDate = Pickup.AddDays(-364);
        var act = () => f.Sut.CreateBooking(f.Request());
        act.Should().Throw<BookingRejectedException>().WithMessage("License must be held for at least 1 year");
    }

    [Fact, Trait("Rule", "BR-14")]
    public void CreateBooking_rejects_missing_vehicle()
    {
        var f = new Fixture(); f.VehicleStore.Vehicle = null;
        var act = () => f.Sut.CreateBooking(f.Request(vehicleId: 404));
        act.Should().Throw<VehicleNotFoundException>().WithMessage("Vehicle #404 not found");
    }

    [Fact, Trait("Rule", "BR-15")]
    public void CreateBooking_rejects_unavailable_vehicle()
    {
        var f = new Fixture(); f.Vehicle.IsAvailable = false;
        var act = () => f.Sut.CreateBooking(f.Request());
        act.Should().Throw<BookingRejectedException>().WithMessage("Vehicle is not available");
    }

    [Theory, Trait("Rule", "BR-16")]
    [InlineData(24, 5, false)] [InlineData(25, 2, false)] [InlineData(25, 3, true)]
    public void CreateBooking_enforces_luxury_age_and_license(int age, int licenseYears, bool accepted)
    {
        var f = new Fixture(); f.Customer.DateOfBirth = Now.AddYears(-age); f.Customer.LicenseIssueDate = Now.AddYears(-licenseYears); f.Vehicle.Category = VehicleCategory.Luxury;
        var act = () => f.Sut.CreateBooking(f.Request());
        if (accepted) act.Should().NotThrow();
        else act.Should().Throw<BookingRejectedException>().WithMessage("Luxury vehicles require age 25+ and 3 years of license");
    }

    [Theory, Trait("Rule", "BR-17")]
    [InlineData(1, false)] [InlineData(2, true)]
    public void CreateBooking_enforces_van_license_years(int years, bool accepted)
    {
        var f = new Fixture(); f.Vehicle.Category = VehicleCategory.Van; f.Customer.LicenseIssueDate = Now.AddYears(-years);
        var act = () => f.Sut.CreateBooking(f.Request());
        if (accepted) act.Should().NotThrow(); else act.Should().Throw<BookingRejectedException>().WithMessage("Vans require 2 years of license");
    }

    [Theory, Trait("Rule", "BR-18")]
    [InlineData(1, 59, false)] [InlineData(2, 0, true)]
    public void CreateBooking_requires_pickup_two_hours_ahead(int hours, int minutes, bool accepted)
    {
        var f = new Fixture(); var request = f.Request(Now.AddHours(hours).AddMinutes(minutes));
        var act = () => f.Sut.CreateBooking(request);
        if (accepted) act.Should().NotThrow(); else act.Should().Throw<BookingRejectedException>().WithMessage("Pickup must be at least 2 hours from now");
    }

    [Theory, Trait("Rule", "BR-19")]
    [InlineData(0, false)] [InlineData(30, true)] [InlineData(31, false)]
    public void CreateBooking_requires_positive_duration_up_to_thirty_days(int days, bool accepted)
    {
        var f = new Fixture(); var request = f.Request(returnAt: f.Pickup.AddDays(days));
        var act = () => f.Sut.CreateBooking(request);
        if (accepted) act.Should().NotThrow(); else act.Should().Throw<BookingRejectedException>().WithMessage("Rental period is invalid");
    }

    [Theory, Trait("Rule", "BR-20")]
    [InlineData(-1, false)] [InlineData(0, true)] [InlineData(3, true)] [InlineData(4, false)]
    public void CreateBooking_allows_zero_through_three_additional_drivers(int count, bool accepted)
    {
        var f = new Fixture(); var act = () => f.Sut.CreateBooking(f.Request(drivers: count));
        if (accepted) act.Should().NotThrow(); else act.Should().Throw<BookingRejectedException>().WithMessage("Up to 3 additional drivers allowed");
    }

    [Theory, Trait("Rule", "BR-21")]
    [InlineData(-1, false)] [InlineData(1, true)] [InlineData(2, true)]
    public void CreateBooking_rejects_overlaps_and_allows_back_to_back(int existingOffsetHours, bool accepted)
    {
        var f = new Fixture(); var pickup = f.Pickup.AddHours(existingOffsetHours);
        f.BookingStore.Bookings.Add(new Booking { VehicleId = 10, PickupAt = f.Pickup, ReturnAt = f.Pickup.AddHours(1), Status = BookingStatus.Reserved });
        var act = () => f.Sut.CreateBooking(f.Request(pickup, pickup.AddDays(2)));
        if (accepted) act.Should().NotThrow(); else act.Should().Throw<BookingRejectedException>().WithMessage("Vehicle is already booked for these dates");
    }

    [Fact, Trait("Rule", "BR-22")]
    public void CreateBooking_rejects_customer_with_two_open_bookings()
    {
        var f = new Fixture(); f.BookingStore.OpenCustomerCount = 2;
        var act = () => f.Sut.CreateBooking(f.Request());
        act.Should().Throw<BookingRejectedException>().WithMessage("Customer already has 2 active bookings");
    }

    [Fact, Trait("Rule", "BR-23")]
    public void CreateBooking_delegates_price_calculation()
    {
        var f = new Fixture(); f.Sut.CreateBooking(f.Request());
        f.Pricing.Calls.Should().Be(1);
    }

    [Fact, Trait("Rule", "BR-24")]
    public void CreateBooking_sets_reserved_status_time_and_start_mileage()
    {
        var f = new Fixture(); f.Vehicle.Mileage = 12345;
        var booking = f.Sut.CreateBooking(f.Request());
        booking.Status.Should().Be(BookingStatus.Reserved); booking.CreatedAt.Should().Be(Now); booking.StartMileage.Should().Be(12345);
    }

    [Fact, Trait("Rule", "BR-25")]
    public void CreateBooking_uses_the_injected_reference_generator()
    {
        var f = new Fixture(); var booking = f.Sut.CreateBooking(f.Request());
        booking.Reference.Should().Be("DN-20261115-AB12"); f.References.LastPickup.Should().Be(f.Pickup);
    }

    [Theory, Trait("Rule", "BR-26")]
    [InlineData(VehicleCategory.Economy, InsurancePlan.Basic, LoyaltyTier.None, 1000)]
    [InlineData(VehicleCategory.SUV, InsurancePlan.Premium, LoyaltyTier.Silver, 1250)]
    [InlineData(VehicleCategory.Luxury, InsurancePlan.Basic, LoyaltyTier.Platinum, 0)]
    public void CreateBooking_calculates_deposit_and_skips_gateway_for_platinum(VehicleCategory category, InsurancePlan plan, LoyaltyTier tier, decimal expected)
    {
        var f = new Fixture(); f.Vehicle.Category = category; f.Customer.LoyaltyTier = tier;
        f.Sut.CreateBooking(f.Request(insurance: plan));
        f.Payments.Charges.Should().Be(expected == 0 ? 0 : 1);
    }

    [Fact, Trait("Rule", "BR-26")]
    public void CreateBooking_does_not_persist_when_deposit_charge_fails()
    {
        var f = new Fixture(); f.Payments.ThrowOnCharge = true;
        var act = () => f.Sut.CreateBooking(f.Request());
        act.Should().Throw<InvalidOperationException>(); f.BookingStore.Bookings.Should().BeEmpty();
    }

    [Fact, Trait("Rule", "BR-27")]
    public void CreateBooking_increments_promo_persists_and_sends_confirmation()
    {
        var f = new Fixture(); var promo = new PromoCode { Code = "SAVE", UsedCount = 2 }; f.PromoStore.Promo = promo;
        var booking = f.Sut.CreateBooking(f.Request(promo: "SAVE"));
        promo.UsedCount.Should().Be(3); f.BookingStore.Bookings.Should().ContainSingle();
        f.Sms.Messages.Should().ContainSingle().Which.Should().Be($"Booking {booking.Reference} confirmed");
        f.Email.Subjects.Should().ContainSingle().Which.Should().Be($"Booking {booking.Reference} confirmed");
        f.Audit.Messages.Should().ContainSingle().Which.Should().Be($"Booking {booking.Reference} created");
    }

    [Fact]
    public void CreateBooking_moq_verifies_external_payment_boundary()
    {
        var f = new Fixture(); var payment = new Mock<IPaymentGateway>(); payment.Setup(p => p.Charge(1, 1000m, It.IsAny<string>())).Returns("ok");
        var sut = f.WithPayment(payment.Object); var booking = sut.CreateBooking(f.Request());
        payment.Verify(p => p.Charge(1, 1000m, $"Deposit {booking.Reference}"), Times.Once);
    }

    [Theory]
    [InlineAutoData]
    public void CreateBooking_autofixture_generated_unrelated_request_fields_are_accepted(int drivers)
    {
        var f = new Fixture(); var request = f.Request(drivers: (int)((uint)drivers % 4), promo: null);
        var booking = f.Sut.CreateBooking(request);
        booking.CustomerId.Should().Be(1); booking.VehicleId.Should().Be(10);
    }

    BookingRequest Request(DateTime? pickup = null, DateTime? returnAt = null, int customerId = 1, int vehicleId = 10, int drivers = 0, string? promo = null, InsurancePlan insurance = InsurancePlan.Basic) =>
        new() { CustomerId = customerId, VehicleId = vehicleId, PickupAt = pickup ?? Pickup, ReturnAt = returnAt ?? Pickup.AddDays(2), AdditionalDrivers = drivers, PromoCode = promo, InsurancePlan = insurance };

    DateTime Pickup => Now.AddDays(10);

    sealed class Fixture
    {
        public readonly StubCustomerRepository CustomerStore = new();
        public readonly StubVehicleRepository VehicleStore = new();
        public readonly FakeBookingRepository BookingStore = new();
        public readonly StubPromoRepository PromoStore = new();
        public readonly StubPricingService Pricing = new();
        public readonly SpyPaymentGateway Payments = new();
        public readonly SpySmsSender Sms = new();
        public readonly SpyEmailSender Email = new();
        public readonly SpyAuditLogger Audit = new();
        public readonly StubClock Clock = new(Now);
        public readonly StubReferenceGenerator References = new();
        public Customer Customer => CustomerStore.Customer!;
        public Vehicle Vehicle => VehicleStore.Vehicle!;
        public DateTime Pickup => Now.AddDays(10);
        public BookingService Sut => WithPayment(Payments);
        public Fixture() { CustomerStore.Customer = new Customer { Id=1, DateOfBirth=Now.AddYears(-34), LicenseIssueDate=Now.AddYears(-10), LoyaltyTier=LoyaltyTier.None, Email="a@b", Phone="010" }; VehicleStore.Vehicle = new Vehicle { Id=10, Category=VehicleCategory.Economy, DailyRate=500, Mileage=1000, IsAvailable=true }; }
        public BookingService WithPayment(IPaymentGateway payment) => new(CustomerStore, VehicleStore, BookingStore, PromoStore, Pricing, payment, Sms, Email, Audit, Clock, References);
        public BookingRequest Request(DateTime? pickup=null, DateTime? returnAt=null, int customerId=1, int vehicleId=10, int drivers=0, string? promo=null, InsurancePlan insurance=InsurancePlan.Basic) => new() { CustomerId=customerId, VehicleId=vehicleId, PickupAt=pickup??Pickup, ReturnAt=returnAt??Pickup.AddDays(2), AdditionalDrivers=drivers, PromoCode=promo, InsurancePlan=insurance };
    }

    sealed class StubClock(DateTime now) : IClock { public DateTime Now { get; } = now; public DateTime Today => Now.Date; }
    sealed class StubCustomerRepository : ICustomerRepository { public Customer? Customer; public Customer? GetById(int id) => Customer?.Id == id ? Customer : null; public void Update(Customer c) => Customer=c; }
    sealed class StubVehicleRepository : IVehicleRepository { public Vehicle? Vehicle; public Vehicle? GetById(int id) => Vehicle?.Id == id ? Vehicle : null; public void Update(Vehicle v) => Vehicle=v; }
    sealed class FakeBookingRepository : IBookingRepository { public List<Booking> Bookings=[]; public int OpenCustomerCount; public Booking? GetById(int id)=>Bookings.FirstOrDefault(b=>b.Id==id); public void Add(Booking b){b.Id=9;Bookings.Add(b);} public void Update(Booking b){} public List<Booking> GetOpenBookingsForVehicle(int id)=>Bookings.Where(b=>b.VehicleId==id && b.Status is BookingStatus.Reserved or BookingStatus.Active).ToList(); public int CountOpenBookingsForCustomer(int id)=>OpenCustomerCount; }
    sealed class StubPromoRepository : IPromoCodeRepository { public PromoCode? Promo; public PromoCode? GetByCode(string c)=>Promo is not null && string.Equals(Promo.Code,c,StringComparison.OrdinalIgnoreCase)?Promo:null; public void Update(PromoCode p)=>Promo=p; }
    sealed class StubPricingService : IPricingService { public int Calls; public PriceBreakdown Calculate(Customer c,Vehicle v,BookingRequest r){Calls++; return new PriceBreakdown { RentalDays=2, Total=1000m };} }
    sealed class SpyPaymentGateway : IPaymentGateway { public int Charges; public bool ThrowOnCharge; public string Charge(int id,decimal amount,string desc){if(ThrowOnCharge)throw new InvalidOperationException("Gateway failed");Charges++;return "ok";} public void Refund(int id,decimal amount,string desc){} }
    sealed class SpySmsSender : ISmsSender { public List<string> Messages=[]; public void Send(string p,string text)=>Messages.Add(text); }
    sealed class SpyEmailSender : IEmailSender { public List<string> Subjects=[]; public void Send(string to,string subject,string body)=>Subjects.Add(subject); }
    sealed class SpyAuditLogger : IAuditLogger { public List<string> Messages=[]; public void Write(string message)=>Messages.Add(message); }
    sealed class StubReferenceGenerator : IBookingReferenceGenerator { public DateTime LastPickup; public string Generate(DateTime pickupAt){LastPickup=pickupAt;return "DN-20261115-AB12";} }
}


