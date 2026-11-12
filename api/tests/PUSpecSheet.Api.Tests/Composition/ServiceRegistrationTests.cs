using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PUSpecSheet.Api.Authorization;
using PUSpecSheet.Api.Collaboration;
using PUSpecSheet.Api.Controllers;
using PUSpecSheet.Application.DependencyInjection;
using PUSpecSheet.Data.DependencyInjection;

namespace PUSpecSheet.Api.Tests.Composition;

/// <summary>
/// Every service the API registers, and every controller, can be built from what is registered, so a
/// repository or service added without its registration fails here rather than on the first request.
/// Nothing connects to a database: building a DbContext doesn't open a connection.
/// </summary>
public sealed class ServiceRegistrationTests
{
    [Fact]
    public void EveryServiceOfOurs_CanBeBuilt()
    {
        var services = Register();
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var scope = provider.CreateScope();

        var ours = services
            .Where(descriptor => descriptor.ServiceType.Namespace?.StartsWith("PUSpecSheet", StringComparison.Ordinal) == true
                && !descriptor.ServiceType.IsGenericTypeDefinition)
            .Select(descriptor => descriptor.ServiceType)
            .Distinct()
            .ToList();

        Assert.NotEmpty(ours);
        foreach (var serviceType in ours)
        {
            Assert.NotNull(scope.ServiceProvider.GetService(serviceType));
        }
    }

    [Fact]
    public void EveryController_CanBeBuilt()
    {
        using var provider = Register().BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var scope = provider.CreateScope();

        var controllers = typeof(PhasesController).Assembly
            .GetTypes()
            .Where(type => type.IsSubclassOf(typeof(ControllerBase)) && !type.IsAbstract)
            .ToList();

        Assert.NotEmpty(controllers);
        foreach (var controller in controllers)
        {
            Assert.NotNull(ActivatorUtilities.CreateInstance(scope.ServiceProvider, controller));
        }
    }

    private static ServiceCollection Register()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddPuSpecSheetData("Server=unused;Database=unused");
        services.AddPuSpecSheetApplication();
        services.AddPuSpecSheetCollaboration(configuration);
        services.AddPuSpecSheetAuthorization();
        return services;
    }
}
