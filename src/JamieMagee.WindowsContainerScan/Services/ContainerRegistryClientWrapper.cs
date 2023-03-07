using Valleysoft.DockerRegistryClient;

namespace JamieMagee.WindowsContainerScan.Services;

internal class ContainerRegistryClientWrapper : IContainerRegistryClient
{
    private readonly RegistryClient registryClient;

    public ContainerRegistryClientWrapper(RegistryClient registryClient)
    {
        this.registryClient = registryClient;
    }

    public IBlobOperations Blobs => registryClient.Blobs;

    public ICatalogOperations Catalog => registryClient.Catalog;

    public IManifestOperations Manifests => registryClient.Manifests;

    public ITagOperations Tags => registryClient.Tags;

    public void Dispose()
    {
        registryClient.Dispose();
    }
}