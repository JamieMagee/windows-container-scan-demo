using JamieMagee.MicrosoftSecurityUpdates.Schema;
using JamieMagee.WindowsContainerScan.Catalogers;
using JamieMagee.WindowsContainerScan.OperatingSystems;
using JamieMagee.WindowsContainerScan.OperatingSystems.Windows;
using JamieMagee.WindowsContainerScan.Services;
using JamieMagee.WindowsContainerScan.Settings;
using JamieMagee.WindowsContainerScan.Sources.DockerRegistry;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace JamieMagee.WindowsContainerScan.Commands;

public sealed class VulnerabilitiesCommand : AsyncCommand<DefaultSettings>
{
    private readonly ICatalogProvider _catalogProvider;
    private readonly IContainerRegistrySource _containerRegistrySource;
    private readonly IIdentifierProvider _identifierProvider;
    private readonly ILogger<VulnerabilitiesCommand> _logger;
    private readonly IMicrosoftSecurityClient _microsoftSecurityClient;

    public VulnerabilitiesCommand(
        IContainerRegistrySource containerRegistrySource,
        IIdentifierProvider identifierProvider,
        ICatalogProvider catalogProvider,
        IMicrosoftSecurityClient microsoftSecurityClient,
        ILogger<VulnerabilitiesCommand> logger)
    {
        _containerRegistrySource = containerRegistrySource;
        _identifierProvider = identifierProvider;
        _catalogProvider = catalogProvider;
        _microsoftSecurityClient = microsoftSecurityClient;
        _logger = logger;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, DefaultSettings settings)
    {
        // Save and unpack the container image
        var location = await _containerRegistrySource.SaveImageAsync(settings.Image);
        var release = await _identifierProvider.IdentifyOperatingSystemAsync(location) as WindowsRelease;
        _logger.LogInformation("Operating System: {OperatingSystem}", release);

        var packages = await _catalogProvider.CatalogAsync(location);
        ShowPackagesTable(packages);

        var vulnerabilityRemediations = await _microsoftSecurityClient.FetchVulnerabilities(release!);
        ShowVulnerabilityTable(vulnerabilityRemediations);

        return 0;
    }

    private static void ShowPackagesTable(IEnumerable<IPackageMetadata> packages)
    {
        var table = new Table();

        table.AddColumn("Package");
        table.AddColumn("Version");

        foreach (var package in packages)
        {
            table.AddRow(package.Package, package.Version);
        }

        AnsiConsole.Write(table);
    }

    private void ShowVulnerabilityTable(IDictionary<string, VulnerabilityRemediation?> vulnerabilityRemediations)
    {
        var table = new Table();

        table.AddColumn("CVE");
        table.AddColumn("Fixed Version");
        table.AddColumn("KB");

        foreach (var (cve, remediation) in vulnerabilityRemediations)
        {
            table.AddRow(
                cve,
                remediation?.FixedBuild ?? string.Empty,
                remediation?.Description.Value ?? string.Empty);
        }

        AnsiConsole.Write(table);
    }
}