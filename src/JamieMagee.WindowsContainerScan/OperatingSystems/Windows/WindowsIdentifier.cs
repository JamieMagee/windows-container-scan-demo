using Registry;

namespace JamieMagee.WindowsContainerScan.OperatingSystems.Windows;

public class WindowsIdentifier : IIdentifier
{
    public string Id => "Windows";

    public string Globs => "**/Files/**/Windows/System32/config/SOFTWARE";

    public async Task<IRelease?> RunAsync(Stream stream)
    {
        // Copy the file to memory
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        
        // Parse the registry
        var hive = new RegistryHiveOnDemand(memoryStream.ToArray(), null);
        // Find the CurrentVersion registry key
        var currentVersion = hive.GetKey(@"Microsoft\Windows NT\CurrentVersion");

        #region ProductName
        // All Windows Server reports as Windows Server 2016
        var productName = currentVersion.GetValue("ProductName").ToString()!.Replace(" Datacenter", string.Empty);
        // Builds after 17622 are Windows Server 2019
        if (int.Parse(currentVersion.GetValue("CurrentBuildNumber").ToString()!) > 17622)
        {
            productName = productName.Remove(productName.Length - 4) + "2019";
        }
        // Builds after 19550 are Windows Server 2022
        if (int.Parse(currentVersion.GetValue("CurrentBuildNumber").ToString()!) > 19550)
        {
            productName = productName.Remove(productName.Length - 4) + "2022";
        }
        #endregion

        return new WindowsRelease
        {
            ProductName = productName,
            MajorVersion = currentVersion.GetValue("CurrentMajorVersionNumber").ToString()!,
            MinorVersion = currentVersion.GetValue("CurrentMinorVersionNumber").ToString()!,
            BuildNumber = currentVersion.GetValue("CurrentBuildNumber").ToString()!,
            BuildRevision = currentVersion.GetValue("UBR").ToString()!
        };
    }
}