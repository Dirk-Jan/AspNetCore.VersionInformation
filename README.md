# AspNetCore.VersionInformation
Small library providing version endpoint and version logging compatible with GitVersion.

It tries to read the version information from the `InformationalVersion` attribute in the project file. If no SemVer can be parsed from the `InformationalVersion` attribute value, it will fall back to the assembly version.

Repository: [Dirk-Jan/AspNetCore.VersionInformation (GitHub.com)](https://github.com/Dirk-Jan/AspNetCore.VersionInformation)

Package: [Dirk-Jan.AspNetCore.VersionInformation (NuGet.org)](https://www.nuget.org/packages/Dirk-Jan.AspNetCore.VersionInformation)

## Getting Started

### Installation

Add the version information service to your dependency injection container in `Program.cs`:

```csharp
using DirkJan.AspNetCore.VersionInformation;

var builder = WebApplicationBuilder.CreateBuilder(args);

builder.Services.AddVersionInformation();

var app = builder.Build();
```

### Getting Version Information

#### Via HTTP Endpoint

Map the version information endpoint to expose version data via HTTP:

```csharp
var app = builder.Build();

app.MapVersionInformation(); // Maps to /version by default

app.Run();
```

You can customize the endpoint path:

```csharp
app.MapVersionInformation("/api/version");
```

The endpoint returns JSON with version details:

```json
{
  "name": "MyApp",
  "semVer": "1.2.3-beta",
  "majorMinorPatch": "1.2.3",
  "major": 1,
  "minor": 2,
  "patch": 3,
  "preReleaseTag": "beta",
  "sha": "abc1234567890def1234567890def1234567890",
  "shortSha": "abc1234",
  "branch": "main",
  "assemblyVersion": "1.2.3.0",
  "informationalVersion": "1.2.3-beta+branch.main.sha.abc1234567890def1234567890def1234567890"
}
```

#### Via Logging

Log the version information when the application starts:

```csharp
var app = builder.Build();

app.LogVersion(); // Uses default log level (Information)

app.Run();
```

You can customize the log level:

```csharp
app.LogVersion(LogLevel.Debug);
```

Or extract a custom version string:

```csharp
app.LogVersion(LogLevel.Information, info => info.SemVer);
```
By default it logs `SemVer`, falling back to `InformationalVersion` or `AssemblyVersion`.

#### Programmatic Access

Inject and use `IVersionInformationService` in your code:

```csharp
public class MyController(IVersionInformationService versionService) : ControllerBase
{
    [HttpGet("my-version")]
    public VersionInfo GetVersion()
    {
        return versionService.GetVersionInformation();
    }
}
```
