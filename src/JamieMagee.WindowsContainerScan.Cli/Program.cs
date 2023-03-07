using JamieMagee.WindowsContainerScan;
using JamieMagee.WindowsContainerScan.Commands;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Extensions.DependencyInjection;

var serviceCollection = new ServiceCollection()
    .AddLogging(
        loggingBuilder => loggingBuilder.AddSerilog(
            new LoggerConfiguration()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .CreateLogger()))
    .AddWindowsContainerScan();
using var registrar = new DependencyInjectionRegistrar(serviceCollection);






















var app = new CommandApp(registrar);
app.Configure(
    config =>
    {
        config.Settings.ApplicationName = "windows-container-scan";
        config.CaseSensitivity(CaseSensitivity.None);

        config.AddCommand<OperatingSystemCommand>("os")
            .WithDescription("Identify the Windows OS version of a container image")
            .WithExample(new[] { "os", "mcr.microsoft.com/windows/nanoserver:ltsc2022" })
            .WithExample(new[] { "os", "docker.io/library/python:3.11" });





















        config.AddCommand<PatchCommand>("patches")
            .WithDescription("Identify patches (KB) installed on a container image")
            .WithExample(new[] { "patches", "mcr.microsoft.com/windows/nanoserver:ltsc2022" })
            .WithExample(new[] { "patches", "docker.io/library/python:3.11" });





















        config.AddCommand<VulnerabilitiesCommand>("vulnerabilities")
            .WithDescription("Identify vulnerabilities (CVE) that affect a container image")
            .WithExample(new[] { "vulnerabilities", "mcr.microsoft.com/windows/nanoserver:ltsc2022" })
            .WithExample(new[] { "vulnerabilities", "docker.io/library/python:3.11" });
    });
return await app.RunAsync(args);
