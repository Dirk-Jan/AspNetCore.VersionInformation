using Microsoft.Extensions.DependencyInjection;

namespace DirkJan.AspNetCore.VersionInformation;

/// <summary>
/// Exception thrown when version information dependencies are not registered in the dependency injection container.
/// </summary>
internal class DependenciesNotRegisteredException() : Exception($"Could not load {nameof(VersionInformation)} dependencies. Please, make sure to call {nameof(Extensions.AddVersionInformation)} on the {nameof(IServiceCollection)} to register the {nameof(VersionInformation)} dependencies.");