namespace JamieMagee.WindowsContainerScan.OperatingSystems;

public interface IIdentifierProvider
{
    /// <summary>
    /// Identify the OS of a container image filesystem
    /// </summary>
    /// <param name="location">The location of the container filesystem</param>
    /// <returns>The OS information</returns>
    Task<IRelease?> IdentifyOperatingSystemAsync(string location);
}
