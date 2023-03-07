namespace JamieMagee.WindowsContainerScan.Sources;

public interface ISource
{
    Task<string> SaveImageAsync(string image, CancellationToken cancellationToken = default);
}
