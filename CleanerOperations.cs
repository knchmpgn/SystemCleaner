using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SystemCleaner.Helpers
{
    /// <summary>
    /// Provides methods to perform various system cleaning operations.
    /// Many of these operations require administrative privileges to be fully effective.
    /// </summary>
    public static class CleanerOperations
    {
        /// <summary>
        /// Recursively removes empty directories under the specified root. System
        /// directories such as Windows and Program Files are skipped to avoid
        /// accidental deletion.
        /// </summary>
        public static Task RemoveEmptyDirectoriesAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    string root = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
                    // Directories that should never be deleted
                    var skip = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        root,
                        Path.Combine(root, "Windows"),
                        Path.Combine(root, "Program Files"),
                        Path.Combine(root, "Program Files (x86)"),
                        Path.Combine(root, "ProgramData"),
                        Path.Combine(root, "Users"),
                        Path.Combine(root, "$Recycle.Bin"),
                        Path.Combine(root, "System Volume Information"),
                    };

                    void ScanDirectory(string dir)
                    {
                        try
                        {
                            if (skip.Any(s => dir.Equals(s, StringComparison.OrdinalIgnoreCase)))
                                return;

                            foreach (var sub in Directory.EnumerateDirectories(dir))
                            {
                                ScanDirectory(sub);
                            }
                            // After scanning subdirectories, see if directory is empty
                            bool hasFiles = Directory.EnumerateFiles(dir).Any();
                            bool hasDirs = Directory.EnumerateDirectories(dir).Any();
                            if (!hasFiles && !hasDirs)
                            {
                                Directory.Delete(dir, false);
                            }
                        }
                        catch
                        {
                            // ignore any access errors
                        }
                    }

                    // Start scanning from the root of the system drive
                    foreach (var dir in Directory.EnumerateDirectories(root))
                    {
                        ScanDirectory(dir);
                    }
                }
                catch
                {
                    // swallow exceptions to avoid crashing the app
                }
            });
        }

        /// <summary>
        /// Deletes user-scoped temporary files. System cache cleanup is handled by
        /// dedicated system operations to avoid overlap.
        /// </summary>
        public static Task RemoveJunkFilesAsync()
        {
            return Task.Run(() =>
            {
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var locations = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    Path.GetTempPath(),
                    Path.Combine(localAppData, "Temp"),
                };

                DeleteDirectoryTargets(locations);
            });
        }

        /// <summary>
        /// Removes broken shortcuts (.lnk files) from common locations. Shortcuts
        /// whose target no longer exists are deleted. Internet shortcuts (.url)
        /// are ignored.
        /// </summary>
        public static Task RemoveBrokenShortcutsAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    var shortcutDirs = new List<string>
                    {
                        Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                        Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                        Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "Internet Explorer", "Quick Launch"),
                    };
                    foreach (var dir in shortcutDirs)
                    {
                        if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                        {
                            foreach (var file in Directory.EnumerateFiles(dir, "*.lnk", SearchOption.AllDirectories))
                            {
                                try
                                {
                                    string? target = GetShortcutTarget(file);
                                    if (string.IsNullOrWhiteSpace(target) || (!File.Exists(target) && !Directory.Exists(target)))
                                    {
                                        File.Delete(file);
                                    }
                                }
                                catch
                                {
                                    // ignore
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Clears recent documents, Jump Lists and Run command histories.
        /// </summary>
        public static Task ClearFileHistoryAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    // Delete recent documents
                    string recent = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "Windows", "Recent");
                    if (Directory.Exists(recent))
                    {
                        DeleteDirectoryContents(recent);
                    }

                    // Delete JumpList automatic destinations
                    string autoDest = Path.Combine(recent, "AutomaticDestinations");
                    if (Directory.Exists(autoDest))
                    {
                        DeleteDirectoryContents(autoDest);
                    }
                    // Delete JumpList custom destinations
                    string customDest = Path.Combine(recent, "CustomDestinations");
                    if (Directory.Exists(customDest))
                    {
                        DeleteDirectoryContents(customDest);
                    }

                    // Clear Run MRU list from registry
                    try
                    {
                        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU", writable: true);
                        if (key != null)
                        {
                            foreach (var valueName in key.GetValueNames())
                            {
                                if (!string.Equals(valueName, "MRUList", StringComparison.OrdinalIgnoreCase))
                                {
                                    try { key.DeleteValue(valueName); } catch { }
                                }
                            }
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Deletes system cache locations commonly covered by Windows cleanup.
        /// </summary>
        public static Task CleanSystemTemporaryFilesAsync()
        {
            return Task.Run(() =>
            {
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string? systemRoot = Environment.GetEnvironmentVariable("SystemRoot");
                string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

                var locations = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    Path.Combine(localAppData, "Microsoft", "Windows", "INetCache"),
                };

                if (!string.IsNullOrWhiteSpace(systemRoot))
                {
                    locations.Add(Path.Combine(systemRoot, "Temp"));
                    locations.Add(Path.Combine(systemRoot, "Prefetch"));
                    locations.Add(Path.Combine(systemRoot, "SoftwareDistribution", "Download"));
                    locations.Add(Path.Combine(systemRoot, "Downloaded Program Files"));
                }

                if (!string.IsNullOrWhiteSpace(programData))
                {
                    locations.Add(Path.Combine(programData, "Microsoft", "Windows", "DeliveryOptimization", "Cache"));
                }

                DeleteDirectoryTargets(locations);
            });
        }

        /// <summary>
        /// Empties the recycle bin for the system drive.
        /// </summary>
        public static Task EmptyRecycleBinAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    string systemDrive = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
                    const uint flags = ShErbNoConfirmation | ShErbNoProgressUi | ShErbNoSound;
                    _ = SHEmptyRecycleBin(IntPtr.Zero, systemDrive, flags);
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Deletes diagnostics, error reports and shader cache data.
        /// </summary>
        public static Task RemoveDiagnosticsAndErrorReportsAsync()
        {
            return Task.Run(() =>
            {
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string? systemRoot = Environment.GetEnvironmentVariable("SystemRoot");

                var locations = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    Path.Combine(localAppData, "D3DSCache"),
                    Path.Combine(localAppData, "Microsoft", "Windows", "WER"),
                    Path.Combine(programData, "Microsoft", "Windows", "WER"),
                };

                if (!string.IsNullOrWhiteSpace(systemRoot))
                {
                    locations.Add(Path.Combine(systemRoot, "Logs"));
                }

                DeleteDirectoryTargets(locations);
            });
        }

        /// <summary>
        /// Clears Windows Defender scan history and Defender event logs.
        /// </summary>
        public static Task ClearWindowsDefenderHistoryAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    if (!string.IsNullOrWhiteSpace(programData))
                    {
                        string defenderRoot = Path.Combine(programData, "Microsoft", "Windows Defender");
                        var locations = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                        {
                            Path.Combine(defenderRoot, "Scans", "History"),
                            Path.Combine(defenderRoot, "Support"),
                        };

                        DeleteDirectoryTargets(locations);
                    }

                    // Best effort; failures are expected on systems where channels do not exist.
                    TryRunProcess("wevtutil.exe", "cl \"Microsoft-Windows-Windows Defender/Operational\"");
                    TryRunProcess("wevtutil.exe", "cl \"Microsoft-Windows-Windows Defender/WHC\"");
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Clears icon and thumbnail cache databases.
        /// </summary>
        public static Task ClearVisualCacheAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    ClearIconAndThumbnailCaches();
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Cleans the Windows component store using DISM.
        /// </summary>
        public static Task CleanComponentStoreAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "dism.exe",
                        Arguments = "/Online /Cleanup-Image /StartComponentCleanup /Quiet",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };
                    using var proc = Process.Start(psi);
                    proc?.WaitForExit();
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Removes browsing history and caches for Chromium-based browsers and Firefox.
        /// Enhanced to include form history, local storage, extension caches, and crash reports.
        /// Cookies and login information are preserved by avoiding deletion of cookie database files.
        /// </summary>
        public static Task WipeBrowserDataAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                    // Chromium-based browsers
                    CleanChromiumBasedBrowserRoot(Path.Combine(localAppData, "Google", "Chrome", "User Data"));
                    CleanChromiumBasedBrowserRoot(Path.Combine(localAppData, "Microsoft", "Edge", "User Data"));
                    CleanChromiumBasedBrowserRoot(Path.Combine(localAppData, "BraveSoftware", "Brave-Browser", "User Data"));
                    CleanChromiumBasedBrowserRoot(Path.Combine(localAppData, "Vivaldi", "User Data"));

                    // Opera commonly stores profile data under roaming app data.
                    CleanChromiumBasedBrowserRoot(Path.Combine(roamingAppData, "Opera Software", "Opera Stable"));
                    CleanChromiumBasedBrowserRoot(Path.Combine(roamingAppData, "Opera Software", "Opera GX Stable"));
                    CleanChromiumBasedBrowserRoot(Path.Combine(localAppData, "Opera Software", "Opera Stable"));
                    CleanChromiumBasedBrowserRoot(Path.Combine(localAppData, "Opera Software", "Opera GX Stable"));

                    // Arc (standalone + Store package layouts)
                    CleanChromiumBasedBrowserRoot(Path.Combine(localAppData, "Arc", "User Data"));
                    string arcPackagesRoot = Path.Combine(localAppData, "Packages");
                    if (Directory.Exists(arcPackagesRoot))
                    {
                        foreach (var packageDir in Directory.EnumerateDirectories(arcPackagesRoot, "TheBrowserCompany.Arc*"))
                        {
                            CleanChromiumBasedBrowserRoot(Path.Combine(packageDir, "LocalCache", "Local", "Arc", "User Data"));
                        }
                    }
                    
                    // Firefox-based browsers
                    CleanFirefox();
                    CleanFirefoxVariant("Waterfox");
                    CleanFirefoxVariant("Pale Moon");
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Removes invalid entries from common Run registry keys. Only entries whose
        /// underlying file no longer exists are removed. Both 64‑ and 32‑bit views
        /// of the registry are scanned.
        /// </summary>
        public static Task CleanRegistryAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    var subKeys = new[] {
                        "Software\\Microsoft\\Windows\\CurrentVersion\\Run",
                        "Software\\Microsoft\\Windows\\CurrentVersion\\RunOnce"
                    };
                    // Scan current user
                    foreach (var subKey in subKeys)
                    {
                        CleanRunKey(Registry.CurrentUser, subKey);
                    }
                    // Scan local machine 64‑bit
                    foreach (var subKey in subKeys)
                    {
                        CleanRunKey(Registry.LocalMachine, subKey);
                    }
                    // Scan local machine 32‑bit view on 64‑bit OS
                    try
                    {
                        var hklm32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                        foreach (var subKey in subKeys)
                        {
                            CleanRunKey(hklm32, subKey);
                        }
                    }
                    catch
                    {
                        // ignore if not available
                    }
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Clears Windows font cache files.
        /// </summary>
        public static Task ClearFontCacheAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    TryRunProcess("sc.exe", "stop FontCache");
                    System.Threading.Thread.Sleep(1000);

                    string? systemRoot = Environment.GetEnvironmentVariable("SystemRoot");
                    if (!string.IsNullOrWhiteSpace(systemRoot))
                    {
                        string fontCachePath = Path.Combine(systemRoot, "ServiceProfiles", "LocalService", "AppData", "Local");
                        TryDeleteFilesByPattern(fontCachePath, "*FontCache*.dat");
                        TryDeleteFilesByPattern(fontCachePath, "FNTCACHE.DAT");
                    }

                    TryRunProcess("sc.exe", "start FontCache");
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Clears Microsoft Store cache.
        /// </summary>
        public static Task ClearWindowsStoreCacheAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    TryRunProcess("wsreset.exe", "");
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Removes superseded Windows Updates using DISM.
        /// </summary>
        public static Task CleanWindowsUpdateAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "dism.exe",
                        Arguments = "/Online /Cleanup-Image /StartComponentCleanup /ResetBase /Quiet",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };
                    using var proc = Process.Start(psi);
                    proc?.WaitForExit();
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Clears network location awareness cache.
        /// </summary>
        public static Task ClearNetworkLocationCacheAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\NetworkList\Profiles", writable: true);
                    if (key != null)
                    {
                        foreach (var subKeyName in key.GetSubKeyNames())
                        {
                            try
                            {
                                key.DeleteSubKeyTree(subKeyName);
                            }
                            catch { }
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Clears BITS (Background Intelligent Transfer Service) transfer jobs.
        /// </summary>
        public static Task ClearBITSQueueAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    TryRunProcess("bitsadmin.exe", "/reset /allusers");
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Removes Component-Based Servicing (CBS) logs.
        /// </summary>
        public static Task ClearCBSLogsAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    string? systemRoot = Environment.GetEnvironmentVariable("SystemRoot");
                    if (!string.IsNullOrWhiteSpace(systemRoot))
                    {
                        string cbsLogPath = Path.Combine(systemRoot, "Logs", "CBS");
                        if (Directory.Exists(cbsLogPath))
                        {
                            DeleteDirectoryContents(cbsLogPath);
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Clears Windows Event Logs (Application, System, Security).
        /// </summary>
        public static Task ClearEventLogsAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    var logs = new[] { "Application", "System", "Security" };
                    foreach (var log in logs)
                    {
                        TryRunProcess("wevtutil.exe", $"cl \"{log}\"");
                    }
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Removes Windows Setup logs.
        /// </summary>
        public static Task ClearWindowsSetupLogsAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    string? systemRoot = Environment.GetEnvironmentVariable("SystemRoot");
                    if (!string.IsNullOrWhiteSpace(systemRoot))
                    {
                        var locations = new HashSet<string>
                        {
                            Path.Combine(systemRoot, "Panther"),
                            Path.Combine(systemRoot, "inf", "setupapi.dev.log"),
                            Path.Combine(systemRoot, "inf", "setupapi.app.log"),
                        };

                        foreach (var loc in locations)
                        {
                            try
                            {
                                if (File.Exists(loc))
                                    File.Delete(loc);
                                else if (Directory.Exists(loc))
                                    DeleteDirectoryContents(loc);
                            }
                            catch { }
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Removes crash dump files (memory.dmp, minidumps).
        /// </summary>
        public static Task ClearCrashDumpsAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    string? systemRoot = Environment.GetEnvironmentVariable("SystemRoot");
                    if (!string.IsNullOrWhiteSpace(systemRoot))
                    {
                        // System-level crash dumps
                        TryDeleteFile(Path.Combine(systemRoot, "MEMORY.DMP"));
                        
                        string minidumpPath = Path.Combine(systemRoot, "Minidump");
                        if (Directory.Exists(minidumpPath))
                        {
                            DeleteDirectoryContents(minidumpPath);
                        }
                    }

                    // User-level crash dumps
                    string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string crashDumpPath = Path.Combine(localAppData, "CrashDumps");
                    if (Directory.Exists(crashDumpPath))
                    {
                        DeleteDirectoryContents(crashDumpPath);
                    }
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Clears Performance Monitor data collector sets.
        /// </summary>
        public static Task ClearPerformanceMonitorDataAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    string perfLogsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Microsoft", "Windows", "PLA");
                    if (Directory.Exists(perfLogsPath))
                    {
                        DeleteDirectoryContents(perfLogsPath);
                    }
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Clears Windows clipboard history (Windows 10/11).
        /// </summary>
        public static Task ClearClipboardHistoryAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string clipboardPath = Path.Combine(localAppData, "Microsoft", "Windows", "Clipboard");
                    if (Directory.Exists(clipboardPath))
                    {
                        DeleteDirectoryContents(clipboardPath);
                    }
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Rebuilds Windows Search index (clears and rebuilds).
        /// </summary>
        public static Task RebuildSearchIndexAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    // Stop Windows Search service
                    TryRunProcess("sc.exe", "stop WSearch");
                    System.Threading.Thread.Sleep(2000);

                    // Delete search index files
                    string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    string searchPath = Path.Combine(programData, "Microsoft", "Search", "Data", "Applications", "Windows");
                    if (Directory.Exists(searchPath))
                    {
                        DeleteDirectoryContents(searchPath);
                    }

                    // Restart service
                    TryRunProcess("sc.exe", "start WSearch");
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Clears UserAssist data (program usage tracking).
        /// </summary>
        public static Task ClearUserAssistDataAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\UserAssist", writable: true);
                    if (key != null)
                    {
                        foreach (var subKeyName in key.GetSubKeyNames())
                        {
                            try
                            {
                                using var countKey = key.OpenSubKey($"{subKeyName}\\Count", writable: true);
                                if (countKey != null)
                                {
                                    foreach (var valueName in countKey.GetValueNames())
                                    {
                                        try { countKey.DeleteValue(valueName); } catch { }
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Clears Explorer typed paths history.
        /// </summary>
        public static Task ClearTypedPathsAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\TypedPaths", writable: true);
                    if (key != null)
                    {
                        foreach (var valueName in key.GetValueNames())
                        {
                            try { key.DeleteValue(valueName); } catch { }
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Clears MUI cache (file type association cache).
        /// </summary>
        public static Task ClearMUICacheAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    using var key = Registry.CurrentUser.OpenSubKey(@"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache", writable: true);
                    if (key != null)
                    {
                        foreach (var valueName in key.GetValueNames())
                        {
                            try { key.DeleteValue(valueName); } catch { }
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Clears recent applications list from Start Menu.
        /// </summary>
        public static Task ClearRecentAppsAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\StartPage", writable: true);
                    if (key != null)
                    {
                        try { key.DeleteValue("StartMenu_Start_Time"); } catch { }
                    }

                    string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string recentAppsPath = Path.Combine(localAppData, "Microsoft", "Windows", "Recent", "AutomaticDestinations");
                    if (Directory.Exists(recentAppsPath))
                    {
                        DeleteDirectoryContents(recentAppsPath);
                    }
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Flushes DNS resolver cache.
        /// </summary>
        public static Task FlushDNSCacheAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    TryRunProcess("ipconfig.exe", "/flushdns");
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Clears NetBIOS name cache.
        /// </summary>
        public static Task ClearNetBIOSCacheAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    TryRunProcess("nbtstat.exe", "-R");
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Clears ARP (Address Resolution Protocol) cache.
        /// </summary>
        public static Task ClearARPCacheAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    TryRunProcess("arp.exe", "-d *");
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Clears Windows networking cache (SMB credentials, offline files).
        /// </summary>
        public static Task ClearWindowsNetworkingCacheAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    // Clear SMB mapped connections and stored credentials.
                    TryRunProcess("net.exe", "use * /delete /y");
                    ClearStoredWindowsCredentials();
                    
                    // Clear offline files cache
                    string? systemRoot = Environment.GetEnvironmentVariable("SystemRoot");
                    if (!string.IsNullOrWhiteSpace(systemRoot))
                    {
                        string cscPath = Path.Combine(systemRoot, "CSC");
                        if (Directory.Exists(cscPath))
                        {
                            try { DeleteDirectoryContents(cscPath); } catch { }
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Removes Windows.old folder if it exists.
        /// </summary>
        public static Task RemoveWindowsOldAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    string root = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
                    string windowsOld = Path.Combine(root, "Windows.old");
                    if (Directory.Exists(windowsOld))
                    {
                        TryDeleteDirectory(windowsOld);
                    }
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Removes old driver packages from the driver store.
        /// </summary>
        public static Task CleanDriverStoreAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "pnputil.exe",
                        Arguments = "/delete-driver * /uninstall /force",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };
                    using var proc = Process.Start(psi);
                    proc?.WaitForExit();
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Cleans Windows Installer cache (orphaned MSI files).
        /// </summary>
        public static Task CleanWindowsInstallerCacheAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    string? systemRoot = Environment.GetEnvironmentVariable("SystemRoot");
                    if (!string.IsNullOrWhiteSpace(systemRoot))
                    {
                        string installerPath = Path.Combine(systemRoot, "Installer", "$PatchCache$");
                        if (Directory.Exists(installerPath))
                        {
                            DeleteDirectoryContents(installerPath);
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Deletes hibernation file (hiberfil.sys) and disables hibernation.
        /// </summary>
        public static Task DisableHibernationAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    TryRunProcess("powercfg.exe", "/hibernate off");
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Deletes all but the most recent System Restore point.
        /// </summary>
        public static Task CleanSystemRestorePointsAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "vssadmin.exe",
                        Arguments = "delete shadows /for=C: /oldest /quiet",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };
                    using var proc = Process.Start(psi);
                    proc?.WaitForExit();
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Clears Most Recently Used (MRU) lists across Windows registry.
        /// </summary>
        public static Task ClearMRUListsAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    var mruPaths = new[]
                    {
                        @"Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\LastVisitedPidlMRU",
                        @"Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\OpenSavePidlMRU",
                        @"Software\Microsoft\Windows\CurrentVersion\Explorer\RecentDocs",
                        @"Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU",
                        @"Software\Microsoft\Windows\CurrentVersion\Explorer\Map Network Drive MRU",
                    };

                    foreach (var path in mruPaths)
                    {
                        try
                        {
                            using var key = Registry.CurrentUser.OpenSubKey(path, writable: true);
                            if (key != null)
                            {
                                foreach (var valueName in key.GetValueNames())
                                {
                                    if (!valueName.Equals("MRUList", StringComparison.OrdinalIgnoreCase))
                                    {
                                        try { key.DeleteValue(valueName); } catch { }
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Cleans invalid file extension associations from registry.
        /// </summary>
        public static Task CleanFileExtensionAssociationsAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    using var classesKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes", writable: true);
                    if (classesKey != null)
                    {
                        foreach (var subKeyName in classesKey.GetSubKeyNames())
                        {
                            if (subKeyName.StartsWith("."))
                            {
                                try
                                {
                                    using var extKey = classesKey.OpenSubKey(subKeyName);
                                    if (extKey != null)
                                    {
                                        var progId = extKey.GetValue("")?.ToString();
                                        if (!string.IsNullOrWhiteSpace(progId))
                                        {
                                            using var progIdKey = classesKey.OpenSubKey(progId);
                                            if (progIdKey == null)
                                            {
                                                // ProgID doesn't exist, remove extension association
                                                classesKey.DeleteSubKeyTree(subKeyName);
                                            }
                                        }
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Removes invalid uninstaller registry entries.
        /// </summary>
        public static Task CleanUninstallEntriesAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    var uninstallPaths = new[]
                    {
                        @"Software\Microsoft\Windows\CurrentVersion\Uninstall",
                        @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
                    };

                    foreach (var basePath in uninstallPaths)
                    {
                        try
                        {
                            using var key = Registry.LocalMachine.OpenSubKey(basePath, writable: true);
                            if (key != null)
                            {
                                foreach (var subKeyName in key.GetSubKeyNames())
                                {
                                    try
                                    {
                                        using var appKey = key.OpenSubKey(subKeyName);
                                        if (appKey != null)
                                        {
                                            var uninstallString = appKey.GetValue("UninstallString")?.ToString();
                                            if (!string.IsNullOrWhiteSpace(uninstallString))
                                            {
                                                string exe = ExtractExecutablePath(uninstallString);
                                                if (!string.IsNullOrEmpty(exe) && !File.Exists(exe))
                                                {
                                                    key.DeleteSubKeyTree(subKeyName);
                                                }
                                            }
                                        }
                                    }
                                    catch { }
                                }
                            }
                        }
                        catch { }
                    }
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Cleans orphaned shared DLL references from registry.
        /// </summary>
        public static Task CleanSharedDLLsAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    using var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\SharedDLLs", writable: true);
                    if (key != null)
                    {
                        foreach (var valueName in key.GetValueNames())
                        {
                            try
                            {
                                if (!File.Exists(valueName))
                                {
                                    key.DeleteValue(valueName);
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Removes invalid COM/DCOM component registrations.
        /// </summary>
        public static Task CleanCOMRegistrationsAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    using var clsidKey = Registry.ClassesRoot.OpenSubKey(@"CLSID", writable: true);
                    if (clsidKey != null)
                    {
                        foreach (var clsid in clsidKey.GetSubKeyNames())
                        {
                            try
                            {
                                using var clsidSubKey = clsidKey.OpenSubKey($"{clsid}\\InprocServer32");
                                if (clsidSubKey != null)
                                {
                                    var path = clsidSubKey.GetValue("")?.ToString();
                                    if (!string.IsNullOrWhiteSpace(path) && !File.Exists(Environment.ExpandEnvironmentVariables(path)))
                                    {
                                        clsidKey.DeleteSubKeyTree(clsid);
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            });
        }

        #region Internal helper methods

        private const uint ShErbNoConfirmation = 0x00000001;
        private const uint ShErbNoProgressUi = 0x00000002;
        private const uint ShErbNoSound = 0x00000004;

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, uint dwFlags);

        private static void DeleteDirectoryTargets(IEnumerable<string> paths)
        {
            foreach (var path in paths.Where(p => !string.IsNullOrWhiteSpace(p)))
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        DeleteDirectoryContents(path);
                    }
                }
                catch
                {
                    // continue on exceptions
                }
            }
        }

        private static void DeleteDirectoryContents(string path)
        {
            try
            {
                foreach (var file in Directory.EnumerateFiles(path))
                {
                    try { File.Delete(file); } catch { }
                }
                foreach (var dir in Directory.EnumerateDirectories(path))
                {
                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch
                    {
                        // If unable to delete the directory, attempt to clear its contents instead
                        DeleteDirectoryContents(dir);
                    }
                }
            }
            catch
            {
                // ignore
            }
        }

        private static string? GetShortcutTarget(string shortcutFile)
        {
            try
            {
                Type? type = Type.GetTypeFromProgID("WScript.Shell");
                if (type == null) return null;
                object? shell = Activator.CreateInstance(type);
                if (shell == null) return null;
                var shortcut = (dynamic)shell;
                var link = shortcut.CreateShortcut(shortcutFile);
                string targetPath = link.TargetPath;
                Marshal.ReleaseComObject(link);
                Marshal.ReleaseComObject(shortcut);
                return targetPath;
            }
            catch
            {
                return null;
            }
        }

        private static void CleanChromiumBasedBrowserRoot(string userDataRoot)
        {
            if (!Directory.Exists(userDataRoot))
            {
                return;
            }

            var profiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var dir in Directory.EnumerateDirectories(userDataRoot))
            {
                string name = Path.GetFileName(dir);
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                bool isUserProfile =
                    name.Equals("Default", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("Guest Profile", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("System Profile", StringComparison.OrdinalIgnoreCase) ||
                    name.StartsWith("Profile ", StringComparison.OrdinalIgnoreCase);

                if (isUserProfile)
                {
                    profiles.Add(dir);
                }
            }

            // Opera and some Arc installs store data directly in this root.
            if (LooksLikeChromiumProfile(userDataRoot) || profiles.Count == 0)
            {
                profiles.Add(userDataRoot);
            }

            foreach (var profile in profiles)
            {
                CleanChromiumProfile(profile);
            }

            // Clear browser-level crash reports.
            TryDeleteDirectory(Path.Combine(userDataRoot, "Crash Reports"));
            TryDeleteDirectory(Path.Combine(userDataRoot, "Crashpad"));
        }

        private static bool LooksLikeChromiumProfile(string profilePath)
        {
            return File.Exists(Path.Combine(profilePath, "History")) ||
                   File.Exists(Path.Combine(profilePath, "Web Data")) ||
                   Directory.Exists(Path.Combine(profilePath, "Local Storage")) ||
                   Directory.Exists(Path.Combine(profilePath, "Session Storage")) ||
                   Directory.Exists(Path.Combine(profilePath, "Extensions"));
        }

        private static void CleanChromiumProfile(string profile)
        {
            // Delete history files and databases.
            TryDeleteFile(Path.Combine(profile, "History"));
            TryDeleteFile(Path.Combine(profile, "History-journal"));
            TryDeleteFile(Path.Combine(profile, "History Provider Cache"));
            TryDeleteFile(Path.Combine(profile, "Web Data")); // Form/search history.
            TryDeleteFile(Path.Combine(profile, "Web Data-journal"));

            // Clear caches.
            TryDeleteDirectory(Path.Combine(profile, "Cache"));
            TryDeleteDirectory(Path.Combine(profile, "Code Cache"));
            TryDeleteDirectory(Path.Combine(profile, "GPUCache"));
            TryDeleteDirectory(Path.Combine(profile, "Media Cache"));
            TryDeleteDirectory(Path.Combine(profile, "Service Worker", "CacheStorage"));
            TryDeleteDirectory(Path.Combine(profile, "Service Worker", "ScriptCache"));

            // Clear local/session storage (preserves cookie/login databases).
            TryDeleteDirectory(Path.Combine(profile, "Local Storage"));
            TryDeleteDirectory(Path.Combine(profile, "Session Storage"));

            // Clear extension caches.
            string extensionsPath = Path.Combine(profile, "Extensions");
            if (Directory.Exists(extensionsPath))
            {
                foreach (var extDir in Directory.EnumerateDirectories(extensionsPath))
                {
                    foreach (var verDir in Directory.EnumerateDirectories(extDir))
                    {
                        TryDeleteDirectory(Path.Combine(verDir, "Cache"));
                        TryDeleteFile(Path.Combine(verDir, "_metadata", "verified_contents.json"));
                    }
                }
            }

            // Clear crash reports.
            TryDeleteDirectory(Path.Combine(profile, "Crash Reports"));
            TryDeleteDirectory(Path.Combine(profile, "Crashpad"));

            // Clear downloads history DB (contains downloads record only).
            TryDeleteFile(Path.Combine(profile, "Download Service", "downloads.json"));
        }

        private static void CleanFirefox()
        {
            string profilesRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla", "Firefox", "Profiles");
            CleanFirefoxProfile(profilesRoot);
        }

        private static void CleanFirefoxVariant(string browserName)
        {
            string profilesRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), browserName, "Profiles");
            CleanFirefoxProfile(profilesRoot);
        }

        private static void CleanFirefoxProfile(string profilesRoot)
        {
            if (Directory.Exists(profilesRoot))
            {
                foreach (var profile in Directory.EnumerateDirectories(profilesRoot))
                {
                    // Delete cache directory
                    TryDeleteDirectory(Path.Combine(profile, "cache2"));
                    
                    // Delete site cache (but keep cookies)
                    TryDeleteDirectory(Path.Combine(profile, "storage", "default"));
                    
                    // Remove download history
                    TryDeleteFile(Path.Combine(profile, "downloads.sqlite"));
                    TryDeleteFile(Path.Combine(profile, "downloads.json"));
                    
                    // Remove form history
                    TryDeleteFile(Path.Combine(profile, "formhistory.sqlite"));
                    
                    // Clear local storage
                    TryDeleteFile(Path.Combine(profile, "webappsstore.sqlite"));

                    // Clear browsing history databases (cookies/logins are separate DBs).
                    TryDeleteFile(Path.Combine(profile, "places.sqlite"));
                    TryDeleteFile(Path.Combine(profile, "places.sqlite-shm"));
                    TryDeleteFile(Path.Combine(profile, "places.sqlite-wal"));
                    
                    // Clear extension caches
                    TryDeleteDirectory(Path.Combine(profile, "extension-data"));
                    
                    // Clear crash reports
                    TryDeleteDirectory(Path.Combine(profile, "crashes"));
                    TryDeleteDirectory(Path.Combine(profile, "minidumps"));
                }
            }
        }

        private static void TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch
            {
                // ignore
            }
        }

        private static void TryDeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path)) Directory.Delete(path, true);
            }
            catch
            {
                // ignore
            }
        }

        private static void ClearIconAndThumbnailCaches()
        {
            try
            {
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string explorerCachePath = Path.Combine(localAppData, "Microsoft", "Windows", "Explorer");

                // Legacy icon cache file location.
                TryDeleteFile(Path.Combine(localAppData, "IconCache.db"));

                if (Directory.Exists(explorerCachePath))
                {
                    TryDeleteFilesByPattern(explorerCachePath, "iconcache*.db");
                    TryDeleteFilesByPattern(explorerCachePath, "thumbcache*.db");
                }

                // Request a shell icon cache refresh when available.
                TryRunProcess("ie4uinit.exe", "-ClearIconCache");
            }
            catch
            {
                // ignore
            }
        }

        private static void TryDeleteFilesByPattern(string directory, string pattern)
        {
            try
            {
                foreach (var file in Directory.EnumerateFiles(directory, pattern))
                {
                    TryDeleteFile(file);
                }
            }
            catch
            {
                // ignore
            }
        }

        private static void ClearStoredWindowsCredentials()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "cmdkey.exe",
                    Arguments = "/list",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                using var proc = Process.Start(psi);
                if (proc == null)
                {
                    return;
                }

                string output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                var targets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var line in output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string trimmed = line.Trim();
                    if (!trimmed.StartsWith("Target:", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string target = trimmed.Substring("Target:".Length).Trim();
                    if (!string.IsNullOrWhiteSpace(target))
                    {
                        targets.Add(target);
                    }
                }

                foreach (var target in targets)
                {
                    TryRunProcess("cmdkey.exe", $"/delete:{target}");
                }
            }
            catch
            {
                // ignore
            }
        }

        private static void TryRunProcess(string fileName, string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                using var proc = Process.Start(psi);
                proc?.WaitForExit();
            }
            catch
            {
                // ignore
            }
        }

        private static void CleanRunKey(RegistryKey baseKey, string subKeyPath)
        {
            try
            {
                using var key = baseKey.OpenSubKey(subKeyPath, writable: true);
                if (key == null) return;
                foreach (var valueName in key.GetValueNames())
                {
                    try
                    {
                        var data = key.GetValue(valueName)?.ToString();
                        if (!string.IsNullOrWhiteSpace(data))
                        {
                            string exe = ExtractExecutablePath(data);
                            if (!string.IsNullOrEmpty(exe) && !File.Exists(exe))
                            {
                                key.DeleteValue(valueName);
                            }
                        }
                    }
                    catch
                    {
                        // ignore errors per value
                    }
                }
            }
            catch
            {
                // ignore
            }
        }

        private static string ExtractExecutablePath(string command)
        {
            // Remove any surrounding quotes and arguments. Only return the exe or file part.
            if (string.IsNullOrWhiteSpace(command)) return string.Empty;
            string trimmed = command.Trim();
            if (trimmed.StartsWith("\""))
            {
                // path is quoted
                int endQuote = trimmed.IndexOf('"', 1);
                if (endQuote > 1)
                {
                    return trimmed.Substring(1, endQuote - 1);
                }
            }
            // Not quoted: assume first token up to first space or .exe extension
            int space = trimmed.IndexOf(' ');
            string candidate = space > 0 ? trimmed.Substring(0, space) : trimmed;
            // Remove arguments after .exe
            int exeIndex = candidate.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
            if (exeIndex >= 0)
            {
                return candidate.Substring(0, exeIndex + 4);
            }
            return candidate;
        }

        #endregion
    }
}
