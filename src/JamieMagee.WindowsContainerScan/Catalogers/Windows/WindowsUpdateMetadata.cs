namespace JamieMagee.WindowsContainerScan.Catalogers.Windows;

public sealed record WindowsUpdateMetadata : IPackageMetadata
{
    public string Package { get; set; } = null!;
    public string Version { get; set; } = null!;
}