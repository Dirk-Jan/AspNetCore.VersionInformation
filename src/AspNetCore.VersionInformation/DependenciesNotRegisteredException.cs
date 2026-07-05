using Microsoft.Extensions.DependencyInjection;

namespace DirkJan.AspNetCore.VersionInformation;

internal class DependenciesNotRegisteredException() : Exception($"Could not load {nameof(VersionInformation)} dependencies. Please, make sure to call {nameof(Extensions.AddVersionInformation)} on the {nameof(IServiceCollection)} to register the {nameof(VersionInformation)} dependencies.");