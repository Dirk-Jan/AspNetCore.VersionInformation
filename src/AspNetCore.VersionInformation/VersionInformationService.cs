using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace DirkJan.AspNetCore.VersionInformation;

/// <summary>
/// Service that retrieves and caches version information from the entry assembly.
/// </summary>
internal class VersionInformationService(ILogger<VersionInformationService> logger) : IVersionInformationService
{
    private VersionInfo? _versionInformation;

    /// <summary>
    /// Gets the cached version information, loading it if necessary.
    /// </summary>
    /// <returns>A <see cref="VersionInfo"/> object containing the version details.</returns>
    public VersionInfo GetVersionInformation()
    {
        _versionInformation ??= Load();
        return _versionInformation;
    }

    /// <summary>
    /// Loads version information from the entry assembly.
    /// </summary>
    /// <returns>A <see cref="VersionInfo"/> object with the loaded version information.</returns>
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

    /// <summary>
    /// Attempts to parse semantic version information from the informational version string.
    /// </summary>
    /// <param name="informationalVersion">The informational version string to parse.</param>
    /// <param name="versionInfo">The version information object to populate with parsed values.</param>
    /// <returns>True if semantic version was successfully parsed; otherwise, false.</returns>
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

    /// <summary>
    /// Parses build metadata to extract branch and commit SHA information.
    /// </summary>
    /// <param name="buildMetadata">The build metadata string to parse.</param>
    /// <param name="versionInfo">The version information object to populate with parsed values.</param>
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

    /// <summary>
    /// Parses Git commit SHA from the informational version string when semantic versioning is not available.
    /// </summary>
    /// <param name="informationalVersion">The informational version string to parse.</param>
    /// <param name="versionInfo">The version information object to populate with parsed values.</param>
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