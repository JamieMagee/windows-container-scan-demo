using JamieMagee.WindowsContainerScan.Models;
using JamieMagee.WindowsContainerScan.Services;
using Valleysoft.DockerRegistryClient;
using Valleysoft.DockerRegistryClient.Models;

namespace JamieMagee.WindowsContainerScan.Utils;

internal record ResolvedManifest(
    ManifestInfo ManifestInfo,
    DockerManifestV2 Manifest);

internal static class ManifestHelper
{
    private const string Os = "windows";
    private const string Architecture = "amd64";

    public static async Task<ResolvedManifest> GetResolvedManifestAsync(
        IContainerRegistryClient client,
        ImageName imageName)
    {
        var manifestInfo = await client.Manifests.GetAsync(imageName.Repo, (imageName.Tag ?? imageName.Digest)!);
        if (manifestInfo.Manifest is ManifestList manifestList)
        {
            var manifestRefs = manifestList.Manifests
                .Where(manifest =>
                    manifest.Platform is { Os: Os, Architecture: Architecture });

            var manifestRef = manifestRefs.First();

            if (manifestRef.Digest is null)
            {
                throw new Exception("Digest of resolved manifest is not set.");
            }

            manifestInfo = await client.Manifests.GetAsync(imageName.Repo, manifestRef.Digest);
        }

        if (manifestInfo.Manifest is not DockerManifestV2 manifest)
        {
            throw new NotSupportedException(
                $"The image name '{imageName}' has a media type of '{manifestInfo.MediaType}' which is not supported.");
        }

        return new ResolvedManifest(manifestInfo, manifest);
    }
}