namespace JamieMagee.WindowsContainerScan.Catalogers;

public interface ICataloger
{
    /// <summary>
    /// File patterns to look for
    /// </summary>
    public string Globs { get; }

    Task<IEnumerable<IPackageMetadata>> RunAsync(Stream stream);
}
