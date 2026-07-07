namespace DirkJan.AspNetCore.VersionInformation;

/// <summary>
/// Service for retrieving version information about the running application.
/// </summary>
public interface IVersionInformationService
{
    /// <summary>
    /// Gets the version information of the application.
    /// </summary>
    /// <returns>A <see cref="VersionInfo"/> object containing the version details.</returns>
    VersionInfo GetVersionInformation();
}