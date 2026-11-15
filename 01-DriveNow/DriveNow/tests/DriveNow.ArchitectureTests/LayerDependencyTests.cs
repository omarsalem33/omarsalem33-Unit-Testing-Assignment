using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using FluentAssertions;

namespace DriveNow.ArchitectureTests;

public class LayerDependencyTests
{
    [Fact]
    public void Domain_does_not_reference_application_infrastructure_or_console()
    {
        ReferencedNames(typeof(Booking).Assembly).Should().NotContain(name => name.StartsWith("DriveNow.Application") || name.StartsWith("DriveNow.Infrastructure") || name.StartsWith("DriveNow.ConsoleApp"));
    }

    [Fact]
    public void Application_does_not_reference_infrastructure_or_console()
    {
        ReferencedNames(typeof(BookingService).Assembly).Should().NotContain(name => name.StartsWith("DriveNow.Infrastructure") || name.StartsWith("DriveNow.ConsoleApp"));
    }

    [Fact]
    public void Services_use_service_suffix_and_live_in_application_services()
    {
        typeof(BookingService).Namespace.Should().Be("DriveNow.Application.Services");
        typeof(BookingService).Name.Should().EndWith("Service");
    }

    static string[] ReferencedNames(System.Reflection.Assembly assembly) => assembly.GetReferencedAssemblies().Select(a => a.Name ?? "").ToArray();
}
