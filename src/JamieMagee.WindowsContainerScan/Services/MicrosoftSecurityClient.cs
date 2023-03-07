using JamieMagee.MicrosoftSecurityUpdates.Client;
using JamieMagee.MicrosoftSecurityUpdates.Schema;
using JamieMagee.WindowsContainerScan.OperatingSystems.Windows;
using Microsoft.Extensions.Logging;

namespace JamieMagee.WindowsContainerScan.Services;

public class MicrosoftSecurityClient : IMicrosoftSecurityClient
{
    private readonly MicrosoftSecurityUpdatesClient _client;

    public MicrosoftSecurityClient()
    {
        _client = new MicrosoftSecurityUpdatesClient();
    }

    public async Task<IDictionary<string, VulnerabilityRemediation?>> FetchVulnerabilities(WindowsRelease release)
    {
        // Fetch the latest vulnerability information
        var cvrf = await _client.GetCvrfByIdAsync("2023-Mar");
        
        // Find the 'productId' from the Windows version name
        var productId = cvrf.ProductTree.FullProductName.Single(n => n.Value == release.ProductName).ProductId;
        
        // Find the list of vulnerabilities that affect that Windows version
        var vulnerabilities = cvrf.Vulnerability.Where(v =>
            v.ProductStatuses.Single(s => s.Status == VulnerabilityStatusStatus.FirstAffected).ProductId
                .Contains(productId));
        
        // Build a map of CVE ID to vulnerability remediation (fixed in KBxxxxxx)
        return vulnerabilities.ToDictionary(v => v.Cve,
            v => v.Remediations.FirstOrDefault(r =>
                r.ProductId.Contains(productId) && r.Type == VulnerabilityRemediationType.VendorFix));
    }
}