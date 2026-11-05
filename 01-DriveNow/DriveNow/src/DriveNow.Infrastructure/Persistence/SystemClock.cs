using DriveNow.Domain.Abstractions;
namespace DriveNow.Infrastructure.Persistence;
public sealed class SystemClock : IClock { public DateTime Now => DateTime.Now; public DateTime Today => DateTime.Today; }

