namespace JamieMagee.WindowsContainerScan.OperatingSystems;

public interface IIdentifier
{
    public string Id { get; }
    
    public string Globs { get; }

    Task<IRelease?> RunAsync(Stream stream);
}
