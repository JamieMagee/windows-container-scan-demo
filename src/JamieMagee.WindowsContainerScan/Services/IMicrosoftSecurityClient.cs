using JamieMagee.MicrosoftSecurityUpdates.Schema;
using JamieMagee.WindowsContainerScan.OperatingSystems.Windows;

namespace JamieMagee.WindowsContainerScan.Services;

public interface IMicrosoftSecurityClient
{
    /// <summary>
    /// For a given Windows release, find the vulnerabilities that affect it
    /// </summary>
    /// <param name="release">The Windows release</param>
    /// <returns>A map of CVE ID to vulnerability remediations</returns>
    Task<IDictionary<string, VulnerabilityRemediation?>> FetchVulnerabilities(WindowsRelease release);
}