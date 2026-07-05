namespace DirkJan.AspNetCore.VersionInformation;

public class VersionInfo
{
    public string? Name { get; internal set; }
    public string? SemVer { get; internal set; }
    public string? MajorMinorPatch { get; internal set; }
    public int? Major { get; internal set; }
    public int? Minor { get; internal set; }
    public int? Patch { get; internal set; }
    public string? PreReleaseTag { get; internal set; }
    public string? Sha { get; internal set; }
    public string? ShortSha { get; internal set; }
    /// <summary>
    /// is escaped
    /// </summary>
    public string? Branch { get; internal set; }
    public string? AssemblyVersion { get; internal set; }
    public string? InformationalVersion { get; internal set; }
}