namespace JamieMagee.WindowsContainerScan.Catalogers;

public interface ICatalogProvider
{
    /// <summary>
    /// Find a list of installed packages in a container filesystem
    /// </summary>
    /// <param name="location">The location of the container filesystem</param>
    /// <returns>The list of installed packages</returns>
    Task<IEnumerable<IPackageMetadata>> CatalogAsync(string location);
}
