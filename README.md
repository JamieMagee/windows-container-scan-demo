A small demo application for [Windows containers: The forgotten stepchild][1] at [Container Plumbing Days 2023][2]

## Usage

```
USAGE:
    windows-container-scan [OPTIONS] <COMMAND>

EXAMPLES:
    windows-container-scan os mcr.microsoft.com/windows/nanoserver:ltsc2022
    windows-container-scan os docker.io/library/python:3.11
    windows-container-scan patches mcr.microsoft.com/windows/nanoserver:ltsc2022
    windows-container-scan patches docker.io/library/python:3.11
    windows-container-scan vulnerabilities mcr.microsoft.com/windows/nanoserver:ltsc2022

OPTIONS:
    -h, --help       Prints help information
    -v, --version    Prints version information

COMMANDS:
    os                 Identify the Windows OS version of a container image
    patches            Identify patches (KB) installed on a container image
    vulnerabilities    Identify vulnerabilities (CVE) that affect a container image
```

[1]: https://containerplumbing.org/sessions/2023/windows_containe
[2]: https://containerplumbing.org/