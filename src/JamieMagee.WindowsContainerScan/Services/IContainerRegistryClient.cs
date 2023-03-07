using Valleysoft.DockerRegistryClient;

namespace JamieMagee.WindowsContainerScan.Services;

public interface IContainerRegistryClient : IDisposable
{
    IBlobOperations Blobs { get; }
    ICatalogOperations Catalog { get; }
    IManifestOperations Manifests { get; }
    ITagOperations Tags { get; }
}