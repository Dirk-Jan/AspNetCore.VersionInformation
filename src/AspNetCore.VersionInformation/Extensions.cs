using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DirkJan.AspNetCore.VersionInformation;

public static class Extensions
{
    public static IServiceCollection AddVersionInformation(this IServiceCollection serviceCollection) =>
        serviceCollection.AddSingleton<IVersionInformationService, VersionInformationService>();

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