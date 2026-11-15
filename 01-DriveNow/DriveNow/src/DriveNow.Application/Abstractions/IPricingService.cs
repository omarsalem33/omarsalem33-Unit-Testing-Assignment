using DriveNow.Domain.Entities;
using DriveNow.Application.Models;
namespace DriveNow.Application.Abstractions;
public interface IPricingService { DriveNow.Domain.ValueObjects.PriceBreakdown Calculate(Customer customer, Vehicle vehicle, BookingRequest request); }
