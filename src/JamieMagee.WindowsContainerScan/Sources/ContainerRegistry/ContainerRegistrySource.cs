using System.IO.Compression;
using System.Text;
using ICSharpCode.SharpZipLib.Tar;
using JamieMagee.WindowsContainerScan.Models;
using JamieMagee.WindowsContainerScan.Services;
using JamieMagee.WindowsContainerScan.Utils;
using Microsoft.Extensions.Logging;
using Valleysoft.DockerRegistryClient;

namespace JamieMagee.WindowsContainerScan.Sources.DockerRegistry;

public class ContainerRegistrySource : Source, IContainerRegistrySource
{
    // See https://github.com/opencontainers/image-spec/blob/main/layer.md#whiteouts
    private const string WhiteoutMarkerPrefix = ".wh.";
    private const string OpaqueWhiteoutMarker = ".wh..wh..opq";

    private readonly ILogger<ContainerRegistrySource> _logger;
    private readonly IContainerRegistryClientFactory _registryClientFactory;

    public ContainerRegistrySource(IContainerRegistryClientFactory registryClientFactory,
        ILogger<ContainerRegistrySource> logger)
    {
        _registryClientFactory = registryClientFactory;
        _logger = logger;
    }

    public override async Task<string> SaveImageAsync(string image, CancellationToken cancellationToken = default)
    {
        var imageName = ImageName.Parse(image);
        var client = await _registryClientFactory.GetClientAsync(imageName.Registry);
        this._logger.LogInformation("Fetching manifest for {Image}", image);
        var manifest = (await ManifestHelper.GetResolvedManifestAsync(client, imageName)).Manifest;

        EnsureTempDirs();
        var destPath = EnsureImageDir(image);

        this._logger.LogInformation("{Image} contains {LayersCount} layers", image, manifest.Layers.Length);
        foreach (var (layer, index) in manifest.Layers.Select((layer, index) => (layer, index)))
        {
            this._logger.LogInformation("Extracting layer {LayerIndex}", index);
            var layerName = layer.Digest![(layer.Digest!.IndexOf(':') + 1)..];
            var layerPath = Path.Join(Constants.LayersTempPath, layerName);

            if (Directory.Exists(layerPath))
            {
                _logger.LogInformation($"Using cached layer on disk at {layerPath}");
            }
            else
            {
                _logger.LogInformation("Extracting layer to {LayerPath}", layerPath);
                await using var layerStream = await client.Blobs.GetAsync(imageName.Repo, layer.Digest);
                await ExtractLayerAsync(layerStream, layerPath);
            }

            _logger.LogInformation("Applying layer {LayerName} to {DestPath}", layerName, destPath);
            ApplyLayer(layerPath, destPath);
        }

        return destPath;
    }

    private async Task ExtractLayerAsync(Stream layerStream, string layerDir)
    {
        await using GZipStream gZipStream = new(layerStream, CompressionMode.Decompress);

        // Can't use System.Formats.Tar.TarReader because it fails to read certain types of tarballs:
        // https://github.com/dotnet/runtime/issues/74316#issuecomment-1312227247
        await using TarInputStream tarStream = new(gZipStream, Encoding.UTF8);

        while (true)
        {
            var entry = tarStream.GetNextEntry();

            if (entry is null)
            {
                break;
            }

            if (entry.Name.StartsWith("UtilityVM"))
            {
                continue;
            }

            if (entry.IsDirectory)
            {
                var directoryPath = Path.Combine(layerDir, entry.Name);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                continue;
            }

            var entryName = entry.Name;
            var entryDirName = Path.GetDirectoryName(entryName) ?? string.Empty;
            var entryFileName = Path.GetFileName(entryName);

            foreach (var invalidChar in Path.GetInvalidPathChars())
            {
                entryDirName = entryDirName.Replace(invalidChar, '-');
            }

            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                entryFileName = entryFileName.Replace(invalidChar, '-');
            }

            entryName = Path.Combine(entryDirName, entryFileName);
            await ExtractTarEntry(layerDir, tarStream, entry, entryName);
        }
    }

    private void ApplyLayer(string layerDir, string workingDir)
    {
        var layerFiles = new DirectoryInfo(layerDir).GetFiles("*", SearchOption.AllDirectories);

        foreach (var layerFile in layerFiles)
        {
            var layerFileRelativePath = Path.GetRelativePath(layerDir, layerFile.FullName);
            var layerFileDirName = Path.GetDirectoryName(layerFileRelativePath);

            // If this an OCI opaque whiteout file marker, delete the directory where the file marker
            // is located.
            if (string.Equals(layerFile.Name, OpaqueWhiteoutMarker, StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(layerFileDirName))
                {
                    throw new Exception("The opaque whiteout file marker should not exist in the root directory.");
                }

                var fullDirPath = Path.Combine(workingDir, layerFileDirName);

                if (Directory.Exists(fullDirPath))
                {
                    Directory.Delete(fullDirPath, true);
                }
            }
            // If this is an OCI whiteout file marker, delete the associated file
            else if (layerFile.Name.StartsWith(WhiteoutMarkerPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var actualFileName = layerFile.Name[WhiteoutMarkerPrefix.Length..];
                var fullFilePath = Path.Combine(
                    workingDir,
                    Path.GetDirectoryName(layerFileDirName) ?? string.Empty,
                    actualFileName);

                if (File.Exists(fullFilePath))
                {
                    File.Delete(fullFilePath);
                }
            }
            else
            {
                var dest = Path.Combine(workingDir, layerFileRelativePath);
                var destDir = Path.GetDirectoryName(dest)!;
                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                if (layerFile.LinkTarget is not null)
                {
                    if (File.Exists(dest))
                    {
                        File.Delete(dest);
                    }

                    try
                    {
                        File.CreateSymbolicLink(dest, layerFile.LinkTarget);
                    }
                    catch (IOException)
                    {
                        // this._logger.LogError("Failed to create symbolic link: {Message}", e.Message);
                    }
                }
                else
                {
                    File.Copy(layerFile.FullName, dest, true);
                }
            }
        }
    }

    private async Task ExtractTarEntry(string workingDir, TarInputStream tarStream, TarEntry entry,
        string entryName)
    {
        var filePath = Path.Combine(workingDir, entryName);
        var directoryPath = Path.GetDirectoryName(filePath);
        if (directoryPath is not null && !Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        if (entry.TarHeader.TypeFlag is TarHeader.LF_LINK or TarHeader.LF_SYMLINK &&
            !string.IsNullOrEmpty(entry.TarHeader.LinkName))
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            try
            {
                File.CreateSymbolicLink(filePath, entry.TarHeader.LinkName);
            }
            catch (IOException e) when (e.Message.StartsWith("A required privilege is not held by the client"))
            {
                // Ignore permissions failures on Windows
            }
        }
        else
        {
            await using var outputStream = File.Create(filePath);
            await tarStream.CopyEntryContentsAsync(outputStream, CancellationToken.None);
        }
    }
}