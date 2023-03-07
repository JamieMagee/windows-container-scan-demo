using System.Text.Json.Serialization;

namespace JamieMagee.WindowsContainerScan.OperatingSystems.Windows;

public sealed record WindowsRelease : IRelease
{
    [JsonPropertyName("productName")]
    public string ProductName { get; init; } = null!;
    
    [JsonPropertyName("majorVersion")]
    public string MajorVersion { get; init; } = null!;

    [JsonPropertyName("minorVersion")]
    public string MinorVersion { get; init; } = null!;

    [JsonPropertyName("majorVersion")]
    public string BuildNumber { get; init; } = null!;

    [JsonPropertyName("buildRevision")]
    public string BuildRevision { get; init; } = null!;

    public override string ToString() =>
        $"{this.ProductName} ({this.MajorVersion}.{this.MinorVersion}.{this.BuildNumber}.{this.BuildRevision})";
}
