using JamieMagee.WindowsContainerScan.Catalogers;
using JamieMagee.WindowsContainerScan.Catalogers.Windows;
using JamieMagee.WindowsContainerScan.Commands;
using JamieMagee.WindowsContainerScan.OperatingSystems;
using JamieMagee.WindowsContainerScan.OperatingSystems.Windows;
using JamieMagee.WindowsContainerScan.Services;
using JamieMagee.WindowsContainerScan.Settings;
using JamieMagee.WindowsContainerScan.Sources.DockerRegistry;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace JamieMagee.WindowsContainerScan;

public static class WindowsContainerScanner
{
    public static IServiceCollection AddWindowsContainerScan(this IServiceCollection services)
    {
        services.AddSingleton<IContainerRegistrySource, ContainerRegistrySource>();

        services.AddSingleton<IContainerRegistryClientFactory, ContainerRegistryClientFactory>();

        services.AddSingleton<IMicrosoftSecurityClient, MicrosoftSecurityClient>();

        services.AddSingleton<ICatalogProvider, CatalogProvider>();
        services.AddSingleton<ICataloger, WindowsCataloger>();
        
        services.AddSingleton<IIdentifierProvider, IdentifierProvider>();
        services.AddSingleton<IIdentifier, WindowsIdentifier>();

        return services;
    }
}