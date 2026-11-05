namespace DriveNow.Infrastructure.Persistence;

public sealed class BookingReferenceGenerator : DriveNow.Domain.Abstractions.IBookingReferenceGenerator
{
    public string Generate(DateTime pickupAt)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        Span<char> chars = stackalloc char[4];
        for (var i = 0; i < chars.Length; i++) chars[i] = alphabet[Random.Shared.Next(alphabet.Length)];
        return $"DN-{pickupAt:yyyyMMdd}-{new string(chars)}";
    }
}

