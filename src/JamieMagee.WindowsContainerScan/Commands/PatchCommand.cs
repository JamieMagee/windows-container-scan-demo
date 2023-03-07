using JamieMagee.WindowsContainerScan.Catalogers;
using JamieMagee.WindowsContainerScan.Settings;
using JamieMagee.WindowsContainerScan.Sources.DockerRegistry;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace JamieMagee.WindowsContainerScan.Commands;

public class PatchCommand : AsyncCommand<DefaultSettings>
{
    private readonly IContainerRegistrySource _containerRegistrySource;
    private readonly ICatalogProvider _catalogProvider;
    private readonly ILogger<PatchCommand> _logger;

    public PatchCommand(
        IContainerRegistrySource containerRegistrySource,
        ICatalogProvider catalogProvider,
        ILogger<PatchCommand> logger)
    {
        _containerRegistrySource = containerRegistrySource;
        _catalogProvider = catalogProvider;
        _logger = logger;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, DefaultSettings settings)
    {
        // Save and unpack the container image
        var location = await _containerRegistrySource.SaveImageAsync(settings.Image);
        // Find the installed Windows updates
        var packages = await _catalogProvider.CatalogAsync(location);

        _logger.LogInformation("Found {PackagesCount} packages:", packages.Count());
        ShowPackagesTable(packages);

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
}
