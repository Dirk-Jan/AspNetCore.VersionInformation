using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DirkJan.AspNetCore.VersionInformation;

/// <summary>
/// Extension methods for configuring and using version information services in ASP.NET Core applications.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registers the version information service in the dependency injection container.
    /// </summary>
    /// <param name="serviceCollection">The service collection to register the service with.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddVersionInformation(this IServiceCollection serviceCollection) =>
        serviceCollection.AddSingleton<IVersionInformationService, VersionInformationService>();

    /// <summary>
    /// Logs the version information of the running application.
    /// </summary>
    /// <param name="host">The host to log version information for.</param>
    /// <param name="logLevel">The log level to use for logging. Defaults to Information.</param>
    /// <param name="predicate">An optional function to extract a custom version string from the version information. If not provided, SemVer is preferred, falling back to InformationalVersion or AssemblyVersion.</param>
    /// <exception cref="DependenciesNotRegisteredException">Thrown when version information dependencies are not registered.</exception>
    public static void LogVersion(this IHost host, LogLevel logLevel = LogLevel.Information,
        Func<VersionInfo, string?>? predicate = null)
    {
        var logger = host.Services.GetService<ILogger<VersionInfo>>();
        if (logger is null) return;
        var service = host.Services.GetService<IVersionInformationService>() ??
                      throw new DependenciesNotRegisteredException();
        var versionInformation = service.GetVersionInformation();
        var version = predicate is null
            ? versionInformation.SemVer ?? versionInformation.InformationalVersion ?? versionInformation.AssemblyVersion
            : predicate(versionInformation);
        logger.Log(logLevel, "Running version: {Version}", version);
    }

    /// <summary>
    /// Maps an HTTP GET endpoint that returns the version information of the application.
    /// </summary>
    /// <param name="endpointRouteBuilder">The endpoint route builder to configure.</param>
    /// <param name="pattern">The route pattern for the version information endpoint. Defaults to "/version".</param>
    /// <returns>A route handler builder for further configuration.</returns>
    /// <exception cref="DependenciesNotRegisteredException">Thrown when version information dependencies are not registered.</exception>
    public static RouteHandlerBuilder MapVersionInformation(this IEndpointRouteBuilder endpointRouteBuilder,
        string pattern = "/version")
    {
        return endpointRouteBuilder
            .MapGet(pattern, (IServiceProvider serviceProvider) =>
            {
                var versionInformationService = serviceProvider.GetService<IVersionInformationService>() ??
                                                throw new DependenciesNotRegisteredException();
                return versionInformationService.GetVersionInformation();
            })
            .WithName("GetVersion");
    }
}