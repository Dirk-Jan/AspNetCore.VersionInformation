namespace DirkJan.AspNetCore.VersionInformation;

/// <summary>
/// Contains version information extracted from the application assembly.
/// </summary>
public class VersionInfo
{
    /// <summary>
    /// The name of the assembly.
    /// </summary>
    public string? Name { get; internal set; }

    /// <summary>
    /// The semantic version (SemVer) string.
    /// </summary>
    public string? SemVer { get; internal set; }

    /// <summary>
    /// The major.minor.patch version string.
    /// </summary>
    public string? MajorMinorPatch { get; internal set; }

    /// <summary>
    /// The major version number.
    /// </summary>
    public int? Major { get; internal set; }

    /// <summary>
    /// The minor version number.
    /// </summary>
    public int? Minor { get; internal set; }

    /// <summary>
    /// The patch version number.
    /// </summary>
    public int? Patch { get; internal set; }

    /// <summary>
    /// The pre-release tag (e.g., alpha, beta).
    /// </summary>
    public string? PreReleaseTag { get; internal set; }

    /// <summary>
    /// The full Git commit SHA (40 characters).
    /// </summary>
    public string? Sha { get; internal set; }

    /// <summary>
    /// The short Git commit SHA (7 characters).
    /// </summary>
    public string? ShortSha { get; internal set; }

    /// <summary>
    /// The Git branch name. Some characters like the forward slash are escaped.
    /// </summary>
    public string? Branch { get; internal set; }

    /// <summary>
    /// The assembly version string.
    /// </summary>
    public string? AssemblyVersion { get; internal set; }

    /// <summary>
    /// The assembly informational version string.
    /// </summary>
    public string? InformationalVersion { get; internal set; }
}