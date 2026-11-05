namespace DriveNow.Infrastructure.External;

public class SmsSender : DriveNow.Domain.Abstractions.ISmsSender
{
    public void Send(string phone, string text)
    {
        Thread.Sleep(500);
        Console.WriteLine($"[SMS to {phone}] {text}");
    }
}


