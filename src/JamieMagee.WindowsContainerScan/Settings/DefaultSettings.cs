using System.ComponentModel;
using Spectre.Console.Cli;

namespace JamieMagee.WindowsContainerScan.Settings;

public sealed class DefaultSettings : CommandSettings
{
    [Description("The image to scan")]
    [CommandArgument(0, "[image]")]
    public string Image { get; set; } = null!;
}
