﻿using System.Text.Json.Serialization;

namespace JamieMagee.WindowsContainerScan.Models;

/// <summary>
/// See <see href="https://github.com/moby/moby/blob/master/image/spec/v1.2.md">Docker Image Specification v1.2.0</see>.
/// </summary>
public record Manifest
{
    /// <summary>
    /// Gets or sets the name of the manifest.
    /// </summary>
    [JsonPropertyName("Config")]
    public string Config { get; init; } = null!;

    /// <summary>
    /// A list of references pointing to this image.
    /// </summary>
    [JsonPropertyName("RepoTags")]
    public IEnumerable<string> RepoTags { get; init; } = null!;

    /// <summary>
    /// A list of layers.
    /// </summary>
    [JsonPropertyName("Layers")]
    public IEnumerable<string> Layers { get; init; } = null!;
}
