using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace DirkJan.AspNetCore.VersionInformation;

internal class VersionInformationService(ILogger<VersionInformationService> logger) : IVersionInformationService
{
    private VersionInfo? _versionInformation;

    public VersionInfo GetVersionInformation()
    {
        _versionInformation ??= Load();
        return _versionInformation;
    }

    private VersionInfo Load()
    {
        logger.LogDebug("Loading version information.");
        var assembly = Assembly.GetEntryAssembly() ?? throw new Exception("Could not get entry assembly.");

        var assemblyName = assembly.GetName();
        var versionInformation = new VersionInfo
        {
            Name = assemblyName.Name,
            AssemblyVersion = assemblyName.Version?.ToString(),
            InformationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion,
        };

        if (versionInformation.InformationalVersion is null)
        {
            logger.LogDebug("InformationalVersion is null. Falling back to assembly version.");
            return versionInformation;
        }

        if (!TryParseSemVer(versionInformation.InformationalVersion, versionInformation))
        {
            logger.LogDebug(
                "Could not extract SemVer from from InformationalVersion. Falling back to assembly version with Git commit SHA.");
            ParseGitCommitSha(versionInformation.InformationalVersion, versionInformation);
            return versionInformation;
        }

        logger.LogDebug("Loaded SemVer version information.");
        return versionInformation;
    }

    private bool TryParseSemVer(string informationalVersion, VersionInfo versionInfo)
    {
        const string semVerPattern =
            @"^(?<Major>0|[1-9]\d*)\.(?<Minor>0|[1-9]\d*)\.(?<Patch>0|[1-9]\d*)(?:-(?<PreRelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<BuildMetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$";
        var matches = Regex.Matches(informationalVersion, semVerPattern);
        switch (matches.Count)
        {
            case 0:
                logger.LogDebug(
                    "InformationalVersion did not match SemVer pattern. InformationalVersion: '{InformationalVersion}'.",
                    versionInfo.InformationalVersion);
                return false;
            case > 1:
                logger.LogWarning(
                    "InformationalVersion matched SemVer pattern multiple times. Using first match. InformationalVersion: '{InformationalVersion}'.",
                    versionInfo.InformationalVersion);
                break;
        }

        var match = matches[0];

        var major = match.Groups["Major"].Value;
        var minor = match.Groups["Minor"].Value;
        var patch = match.Groups["Patch"].Value;
        var majorMinorPatch = $"{major}.{minor}.{patch}";
        var preRelease = match.Groups["PreRelease"].Value;
        var semVer = $"{majorMinorPatch}-{preRelease}";
        versionInfo.MajorMinorPatch = majorMinorPatch;
        versionInfo.PreReleaseTag = preRelease;
        versionInfo.SemVer = semVer;
        versionInfo.Major = int.TryParse(major, out var majorInteger) ? majorInteger : null;
        versionInfo.Minor = int.TryParse(minor, out var minorInteger) ? minorInteger : null;
        versionInfo.Patch = int.TryParse(patch, out var patchInteger) ? patchInteger : null;

        var buildMetadata = match.Groups["BuildMetadata"].Value;
        ParseBuildMetadata(buildMetadata, versionInfo);

        return true;
    }

    private void ParseBuildMetadata(string buildMetadata, VersionInfo versionInfo)
    {
        const string buildMetadataPattern =
            @"(?:(?:B|b)ranch\.(?<Branch>.+)\.(?:S|s)ha\.[0-9a-zA-Z]{40}\.)?(?<Sha>[0-9a-zA-Z]{40})$";
        var matches = Regex.Matches(buildMetadata, buildMetadataPattern);
        switch (matches.Count)
        {
            case 0:
                logger.LogDebug("Build metadata did not match pattern. Build metadata: '{BuildMetadata}'.",
                    versionInfo.InformationalVersion);
                return;
            case > 1:
                logger.LogWarning(
                    "Build metadata matched pattern multiple times. Using first match. Build metadata: '{BuildMetadata}'.",
                    versionInfo.InformationalVersion);
                break;
        }

        var branch = matches[0].Groups["Branch"].Value;
        var sha = matches[0].Groups["Sha"].Value;
        versionInfo.Branch = string.IsNullOrWhiteSpace(branch) ? null : branch;
        versionInfo.Sha = sha;
        versionInfo.ShortSha = sha[..7];
    }

    private void ParseGitCommitSha(string informationalVersion, VersionInfo versionInfo)
    {
        const string gitCommitShaPattern = "(?<Sha>[0-9a-zA-Z]{40})$";
        var matches = Regex.Matches(informationalVersion, gitCommitShaPattern);
        switch (matches.Count)
        {
            case 0:
                logger.LogDebug(
                    "InformationalVersion did not match Git commit SHA pattern. InformationalVersion: '{InformationalVersion}'.",
                    versionInfo.InformationalVersion);
                return;
            case > 1:
                logger.LogWarning(
                    "InformationalVersion matched Git commit SHA pattern multiple times. Using first match. InformationalVersion: '{InformationalVersion}'.",
                    versionInfo.InformationalVersion);
                break;
        }

        var sha = matches[0].Groups["Sha"].Value;
        versionInfo.Sha = sha;
        versionInfo.ShortSha = sha[..7];
    }
}