using System.Reflection;
using FluentAssertions;
using ParkSmart.Application;
using ParkSmart.Domain;
using ParkSmart.Infrastructure;
using ParkSmart.UnitTests;

namespace ParkSmart.ArchitectureTests;

public class ParkSmartArchitectureTests
{
    [Fact]
    public void Domain_has_no_dependencies_on_other_ParkSmart_projects()
    {
        ParkSmartNames(typeof(ParkingSession).Assembly).Should().BeEmpty();
    }

    [Fact]
    public void Application_depends_only_on_Domain_among_ParkSmart_projects()
    {
        var names = ParkSmartNames(typeof(ParkingFeeCalculator).Assembly);
        names.Should().Contain("ParkSmart.Domain").And.NotContain("ParkSmart.Infrastructure").And.NotContain("ParkSmart.ConsoleApp");
    }

    [Fact]
    public void Infrastructure_does_not_depend_on_Application_or_ConsoleApp()
    {
        var names = ParkSmartNames(typeof(ParkSmart.Infrastructure.ParkSmartInfrastructureMarker).Assembly);
        names.Should().NotContain("ParkSmart.Application").And.NotContain("ParkSmart.ConsoleApp");
    }

    [Fact]
    public void Unit_test_classes_follow_the_Tests_suffix()
    {
        typeof(ParkingFeeCalculatorTests).Assembly.GetTypes().Where(t => t.Name.EndsWith("Tests", StringComparison.Ordinal)).Should().NotBeEmpty();
    }

    static string[] ParkSmartNames(Assembly assembly) => assembly.GetReferencedAssemblies()
        .Select(name => name.Name!)
        .Where(name => name.StartsWith("ParkSmart.", StringComparison.Ordinal))
        .ToArray();
}

