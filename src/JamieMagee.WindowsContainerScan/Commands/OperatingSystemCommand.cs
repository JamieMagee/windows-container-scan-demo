using JamieMagee.WindowsContainerScan.OperatingSystems;
using JamieMagee.WindowsContainerScan.Settings;
using JamieMagee.WindowsContainerScan.Sources.DockerRegistry;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace JamieMagee.WindowsContainerScan.Commands;

public class OperatingSystemCommand : AsyncCommand<DefaultSettings>
{
    private readonly IContainerRegistrySource _containerRegistrySource;
    private readonly IIdentifierProvider _identifierProvider;
    private readonly ILogger<OperatingSystemCommand> _logger;

    public OperatingSystemCommand(
        IContainerRegistrySource containerRegistrySource,
        IIdentifierProvider identifierProvider,
        ILogger<OperatingSystemCommand> logger
    )
    {
        _containerRegistrySource = containerRegistrySource;
        _identifierProvider = identifierProvider;
        _logger = logger;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, DefaultSettings settings)
    {
        // Save and unpack the container image
        var location = await _containerRegistrySource.SaveImageAsync(settings.Image);
        // Identify the Windows release
        var release = await _identifierProvider.IdentifyOperatingSystemAsync(location);
        _logger.LogInformation("Operating System: {OperatingSystem}", release);

        return 0;
    }
}
