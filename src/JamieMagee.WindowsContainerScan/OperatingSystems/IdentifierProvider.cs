using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Logging;

namespace JamieMagee.WindowsContainerScan.OperatingSystems;

public class IdentifierProvider : IIdentifierProvider
{
    private readonly IEnumerable<IIdentifier> _identifiers;
    private readonly ILogger<IdentifierProvider> _logger;

    public IdentifierProvider(IEnumerable<IIdentifier> identifiers, ILogger<IdentifierProvider> logger)
    {
        _identifiers = identifiers;
        _logger = logger;
    }
    
    public async Task<IRelease?> IdentifyOperatingSystemAsync(string location)
    {
        _logger.LogInformation("Searching for OS filesystem");

        // For each identifier
        foreach (var identifier in _identifiers)
        {
            // See if there are any files that match the patterns it's looking for
            var result = new Matcher()
                .AddInclude(identifier.Globs)
                .Execute(new DirectoryInfoWrapper(new DirectoryInfo(location)));

            if (!result.HasMatches)
            {
                continue;
            }
            
            // Open the file
            await using var fileStream = File.OpenRead(Path.Join(location, result.Files.Single().Path));
            // Run the cataloger
            return await identifier.RunAsync(fileStream);
            
        }

        return null;
    }
}
