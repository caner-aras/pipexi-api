using NetArchTest.Rules;

namespace Pipexi.ArchitectureTests;

public sealed class LayerDependencyTests
{
    [Fact]
    public void Domain_Should_Not_Depend_On_Other_Layers()
    {
        var result = Types
            .InAssembly(typeof(Workforce.Domain.Primitives.DomainMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Workforce.Application",
                "Workforce.Persistence",
                "Workforce.Infrastructure",
                "Workforce.Api")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure_Or_Persistence_Or_Api()
    {
        var result = Types
            .InAssembly(typeof(Workforce.Application.DependencyInjection.ServiceRegistration).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Workforce.Infrastructure",
                "Workforce.Persistence",
                "Workforce.Api")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Contracts_Should_Not_Depend_On_Other_Layers()
    {
        var result = Types
            .InAssembly(typeof(Workforce.Contracts.V1.Auth.LoginRequest).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Workforce.Api",
                "Workforce.Application",
                "Workforce.Domain",
                "Workforce.Persistence",
                "Workforce.Infrastructure",
                "Workforce.Shared")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }
}