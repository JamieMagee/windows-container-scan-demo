namespace JamieMagee.WindowsContainerScan.Catalogers.Windows;

public enum CurrentState
{
    Absent = 0x0,
    UninstallPending = 0x5,
    Resolving = 0x10,
    Resolved = 0x20,
    Staging = 0x30,
    Staged = 0x40,
    Superseded = 0x50,
    InstallPending = 0x60,
    PartiallyInstalled = 0x65,
    Installed = 0x70,
    Permanent = 0x80
}