namespace JamieMagee.WindowsContainerScan.Services;

public interface IContainerRegistryClientFactory
{
    Task<IContainerRegistryClient> GetClientAsync(string? registry);
}