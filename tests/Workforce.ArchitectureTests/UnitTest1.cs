using NetArchTest.Rules;

namespace Pipexi.ArchitectureTests;

public sealed class LayerDependencyTests
{
    [Fact]
    public void Domain_Should_Not_Depend_On_Other_Layers()
    {
        var result = Types
            .InAssembly(typeof(Pipexi.Domain.Primitives.DomainMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Pipexi.Application",
                "Pipexi.Persistence",
                "Pipexi.Infrastructure",
                "Pipexi.Api")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure_Or_Persistence_Or_Api()
    {
        var result = Types
            .InAssembly(typeof(Pipexi.Application.DependencyInjection.ServiceRegistration).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Pipexi.Infrastructure",
                "Pipexi.Persistence",
                "Pipexi.Api")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Contracts_Should_Not_Depend_On_Other_Layers()
    {
        var result = Types
            .InAssembly(typeof(Pipexi.Contracts.V1.Auth.LoginRequest).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Pipexi.Api",
                "Pipexi.Application",
                "Pipexi.Domain",
                "Pipexi.Persistence",
                "Pipexi.Infrastructure",
                "Pipexi.Shared")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }
}