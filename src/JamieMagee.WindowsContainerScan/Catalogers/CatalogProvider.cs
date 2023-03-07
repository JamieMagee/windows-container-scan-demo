using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace JamieMagee.WindowsContainerScan.Catalogers;

public class CatalogProvider : ICatalogProvider
{
    private readonly IEnumerable<ICataloger> _catalogers;

    public CatalogProvider(IEnumerable<ICataloger> catalogers)
    {
        _catalogers = catalogers;
    }

    public async Task<IEnumerable<IPackageMetadata>> CatalogAsync(string location)
    {
        var packages = new List<IPackageMetadata>();

        // For each cataloger
        foreach (var cataloger in _catalogers)
        {
            // See if there are any files that match the patterns it's looking for
            var result = new Matcher()
                .AddInclude(cataloger.Globs)
                .Execute(new DirectoryInfoWrapper(new DirectoryInfo(location)));

            if (!result.HasMatches)
            {
                continue;
            }

            // For each file match
            foreach (var file in result.Files)
            {
                // Open the file
                await using var fileStream = File.OpenRead(Path.Join(location, file.Path));
                // Run the cataloger
                var catalogerResult = await cataloger.RunAsync(fileStream);
                // Add the packages to the list
                packages.AddRange(catalogerResult);
            }
        }

        return packages;
    }
}
