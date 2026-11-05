using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Application.Abstractions;
using DriveNow.Domain.Abstractions;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using FluentAssertions;

namespace DriveNow.UnitTests;

public class PricingServiceTests
{
    readonly PricingService _sut = new(new PromoRepository(), new FixedClock(DateTime.Today));

    [Fact]
    [Trait("Rule", "BR-30")]
    public void Calculate_matches_worked_example_A()
    {
        var pickup = NextWeekday(DateTime.Today, DayOfWeek.Sunday).AddHours(10);
        var result = _sut.Calculate(Customer(34, LoyaltyTier.Gold), Vehicle(600, VehicleCategory.Economy), Request(pickup, pickup.AddDays(3), InsurancePlan.Standard));
        result.RentalDays.Should().Be(3);
        result.BaseAmount.Should().Be(1800m);
        result.LoyaltyDiscount.Should().Be(180m);
        result.InsuranceCost.Should().Be(450m);
        result.Total.Should().Be(2070m);
    }

    [Fact]
    [Trait("Rule", "BR-37")]
    public void Calculate_caps_combined_duration_loyalty_and_promo_discounts()
    {
        var pickup = NextWeekday(DateTime.Today, DayOfWeek.Thursday).AddHours(9);
        var result = _sut.Calculate(Customer(23, LoyaltyTier.Silver), Vehicle(1500, VehicleCategory.SUV), Request(pickup, pickup.AddDays(7).AddMinutes(30), InsurancePlan.Premium, 1, "SAVE15"));
        result.RentalDays.Should().Be(7);
        result.BaseAmount.Should().Be(10500m);
        result.WeekendSurcharge.Should().Be(600m);
        result.YoungDriverSurcharge.Should().Be(1575m);
        result.DurationDiscount.Should().Be(1050m);
        result.LoyaltyDiscount.Should().Be(525m);
        result.PromoDiscount.Should().Be(1050m);
        result.InsuranceCost.Should().Be(2100m);
        result.AdditionalDriversCost.Should().Be(700m);
        result.Total.Should().Be(12850m);
    }

    [Theory]
    [Trait("Rule", "BR-33")]
    [InlineData(6, 0)] [InlineData(7, 10)] [InlineData(13, 10)] [InlineData(14, 15)]
    public void Calculate_applies_duration_discount_bands(int days, decimal percent)
    {
        var pickup = NextWeekday(DateTime.Today, DayOfWeek.Sunday).AddHours(10);
        var result = _sut.Calculate(Customer(34, LoyaltyTier.None), Vehicle(1000, VehicleCategory.Economy), Request(pickup, pickup.AddDays(days), InsurancePlan.Basic));
        result.DurationDiscount.Should().Be(1000m * days * percent / 100m);
    }

    [Theory]
    [Trait("Rule", "BR-32")]
    [InlineData(20, 0)] [InlineData(21, 15)] [InlineData(24, 15)] [InlineData(25, 0)]
    public void Calculate_applies_young_driver_boundary_ages(int age, decimal percent)
    {
        var pickup = NextWeekday(DateTime.Today, DayOfWeek.Sunday).AddHours(10);
        var result = _sut.Calculate(Customer(age, LoyaltyTier.None), Vehicle(1000, VehicleCategory.Economy), Request(pickup, pickup.AddDays(1), InsurancePlan.Basic));
        result.YoungDriverSurcharge.Should().Be(1000m * percent / 100m);
    }


    [Theory]
    [Trait("Rule", "BR-39")]
    [InlineData(LoyaltyTier.None, 100)] [InlineData(LoyaltyTier.Silver, 100)] [InlineData(LoyaltyTier.Gold, 0)] [InlineData(LoyaltyTier.Platinum, 0)]
    public void Calculate_waives_one_additional_driver_only_for_gold_and_platinum(LoyaltyTier tier, decimal expected)
    {
        var pickup = NextWeekday(DateTime.Today, DayOfWeek.Sunday).AddHours(10);
        var result = _sut.Calculate(Customer(34, tier), Vehicle(500, VehicleCategory.Economy), Request(pickup, pickup.AddDays(1), InsurancePlan.Basic, 1));
        result.AdditionalDriversCost.Should().Be(expected);
    }

    [Fact]
    [Trait("Rule", "BR-38")]
    public void Calculate_doubles_insurance_for_luxury_vehicles()
    {
        var pickup = NextWeekday(DateTime.Today, DayOfWeek.Sunday).AddHours(10);
        var result = _sut.Calculate(Customer(34, LoyaltyTier.None), Vehicle(1000, VehicleCategory.Luxury), Request(pickup, pickup.AddDays(2), InsurancePlan.Standard));
        result.InsuranceCost.Should().Be(600m);
    }

    [Theory]
    [Trait("Rule", "BR-36")]
    [InlineData("NO_SUCH_CODE", "Promo code 'NO_SUCH_CODE' does not exist")]
    [InlineData("SUMMER25", "Promo code 'SUMMER25' has expired")]
    [InlineData("WEEKLY20", "Promo code 'WEEKLY20' requires at least 7 days")]
    [InlineData("LUX10", "Promo code 'LUX10' is not valid for SUV")]
    public void Calculate_rejects_invalid_promo_codes_with_rule_message(string code, string expectedMessage)
    {
        var pickup = NextWeekday(DateTime.Today, DayOfWeek.Sunday).AddHours(10);
        var result = () => _sut.Calculate(Customer(34, LoyaltyTier.None), Vehicle(500, VehicleCategory.SUV), Request(pickup, pickup.AddDays(2), InsurancePlan.Basic, promo: code));
        result.Should().Throw<DriveNow.Domain.Exceptions.InvalidPromoCodeException>().WithMessage(expectedMessage);
    }

    [Fact]
    [Trait("Rule", "BR-36")]
    public void Calculate_rejects_a_fully_used_promo_after_category_validation()
    {
        var pickup = NextWeekday(DateTime.Today, DayOfWeek.Sunday).AddHours(10);
        var result = () => _sut.Calculate(Customer(34, LoyaltyTier.None), Vehicle(4000, VehicleCategory.Luxury), Request(pickup, pickup.AddDays(2), InsurancePlan.Basic, promo: "LUX10"));
        result.Should().Throw<DriveNow.Domain.Exceptions.InvalidPromoCodeException>().WithMessage("Promo code 'LUX10' has been fully used");
    }
    static Customer Customer(int age, LoyaltyTier tier) => new()
    {
        Id = age, DateOfBirth = DateTime.Today.AddYears(-age), LicenseIssueDate = DateTime.Today.AddYears(-5), LoyaltyTier = tier
    };

    static Vehicle Vehicle(decimal rate, VehicleCategory category) => new() { DailyRate = rate, Category = category };

    static BookingRequest Request(DateTime pickup, DateTime ret, InsurancePlan insurance, int drivers = 0, string? promo = null) => new()
    {
        PickupAt = pickup, ReturnAt = ret, InsurancePlan = insurance, AdditionalDrivers = drivers, PromoCode = promo
    };

    static DateTime NextWeekday(DateTime from, DayOfWeek day)
    {
        var offset = ((int)day - (int)from.DayOfWeek + 7) % 7;
        return from.Date.AddDays(offset == 0 ? 7 : offset);
    }
    sealed class FixedClock(DateTime today) : IClock { public DateTime Now => today; public DateTime Today => today; }
    sealed class PromoRepository : IPromoCodeRepository
    {
        readonly List<PromoCode> _items = [
            new() { Code="SAVE15", DiscountPercent=15, ValidFrom=DateTime.Today.AddDays(-10), ValidUntil=DateTime.Today.AddDays(60), MinDays=1, MaxUses=100, UsedCount=3 },
            new() { Code="WEEKLY20", DiscountPercent=20, ValidFrom=DateTime.Today.AddDays(-5), ValidUntil=DateTime.Today.AddDays(90), MinDays=7, MaxUses=50 },
            new() { Code="LUX10", DiscountPercent=10, ValidFrom=DateTime.Today.AddDays(-30), ValidUntil=DateTime.Today.AddDays(30), MinDays=2, AllowedCategories=[VehicleCategory.Luxury], MaxUses=20, UsedCount=20 },
            new() { Code="SUMMER25", DiscountPercent=25, ValidFrom=DateTime.Today.AddDays(-120), ValidUntil=DateTime.Today.AddDays(-30), MinDays=1, MaxUses=100 }
        ];
        public PromoCode? GetByCode(string code) => _items.FirstOrDefault(p => string.Equals(p.Code,code,StringComparison.OrdinalIgnoreCase));
        public void Update(PromoCode promo) { }
    }}



