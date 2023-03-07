using System.Text.RegularExpressions;
using Registry;

namespace JamieMagee.WindowsContainerScan.Catalogers.Windows;

public class WindowsCataloger : ICataloger
{
    // Matches "Package_1_for_KB123456..."
    private static readonly Regex UpdatePackageRegex = 
        new(@"^Package_\d+_for_(KB\d+)~\w{16}~\w+~~((?:\d+\.){3}\d+)$");

    // SOFTWARE registry hive
    public string Globs => "**/Windows/System32/config/SOFTWARE";

    public async Task<IEnumerable<IPackageMetadata>> RunAsync(Stream stream)
    {
        // Copy the file stream into memory
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);

        // Parse the registry
        var hive = new RegistryHiveOnDemand(memoryStream.ToArray(), null);

        // Find the installed packages registry key
        var packagesRegistryKey = hive.GetKey(@"Microsoft\Windows\CurrentVersion\Component Based Servicing\Packages");

        var packages = new List<WindowsUpdateMetadata>();
        // For each sub key
        foreach (var key in packagesRegistryKey.SubKeys)
        {
            // Check if it matches the regex
            if (!UpdatePackageRegex.IsMatch(key.KeyName))
            {
                continue;
            }

            // Read the key and find the installed state
            var package = hive.GetKey(key.KeyPath);
            var currentState = package.Values.Find(v => v.ValueName == "CurrentState")?.ValueData;

            // If the package is installed
            if (currentState != null && int.Parse(currentState) == (int)CurrentState.Installed)
            {
                // Parse the key name and add it to the list of installed packages
                var groups = UpdatePackageRegex.Match(package.KeyName).Groups;
                var windowsUpdate = new WindowsUpdateMetadata
                {
                    Package = groups[1].Value,
                    Version = groups[2].Value
                };
                if (!packages.Contains(windowsUpdate))
                {
                    packages.Add(windowsUpdate);
                }
            }
        }


        return packages;
    }
}
