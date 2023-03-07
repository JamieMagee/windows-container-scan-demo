namespace JamieMagee.WindowsContainerScan;

internal static class Constants
{
    internal static readonly string StethoscopeTempPath = Path.Combine(Path.GetTempPath(), "windows-container-scan");
    internal static readonly string LayersTempPath = Path.Combine(StethoscopeTempPath, "layers");
    internal static readonly string ImagesTempPath = Path.Combine(StethoscopeTempPath, "images");
}
