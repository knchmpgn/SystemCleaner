using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Windows;
using SystemCleaner.Helpers;
using SystemCleaner.Models;

namespace SystemCleaner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly CleanerSettings _settings;

        public MainWindow()
        {
            InitializeComponent();
            _settings = CleanerSettings.Load();
            ApplySettingsToUi();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            PersistSettings();
        }

        private void ApplySettingsToUi()
        {
            // Storage
            JunkFilesCheckBox.IsChecked = _settings.RemoveJunkFiles;
            SystemTempCheckBox.IsChecked = _settings.CleanSystemTemporaryFiles;
            RecycleBinCheckBox.IsChecked = _settings.EmptyRecycleBin;
            DiagnosticsCheckBox.IsChecked = _settings.RemoveDiagnosticsAndErrorReports;
            VisualCacheCheckBox.IsChecked = _settings.ClearVisualCache;

            // History and Privacy
            FileHistoryCheckBox.IsChecked = _settings.ClearFileHistory;
            BrowserDataCheckBox.IsChecked = _settings.WipeBrowserData;
            WindowsDefenderCheckBox.IsChecked = _settings.RemoveWindowsDefenderHistory;

            // System
            RegistryCleanupCheckBox.IsChecked = _settings.CleanRegistry;
            ComponentStoreCheckBox.IsChecked = _settings.CleanComponentStore;

            // Clutter
            EmptyDirsCheckBox.IsChecked = _settings.RemoveEmptyDirectories;
            BrokenShortcutsCheckBox.IsChecked = _settings.RemoveBrokenShortcuts;

            // Windows System Cache
            ClearFontCacheCheckBox.IsChecked = _settings.ClearFontCache;
            ClearWindowsStoreCacheCheckBox.IsChecked = _settings.ClearWindowsStoreCache;
            CleanWindowsUpdateCheckBox.IsChecked = _settings.CleanWindowsUpdate;
            ClearNetworkLocationCacheCheckBox.IsChecked = _settings.ClearNetworkLocationCache;
            ClearBITSQueueCheckBox.IsChecked = _settings.ClearBITSQueue;
            ClearCBSLogsCheckBox.IsChecked = _settings.ClearCBSLogs;

            // Event & Diagnostic Logs
            ClearEventLogsCheckBox.IsChecked = _settings.ClearEventLogs;
            ClearWindowsSetupLogsCheckBox.IsChecked = _settings.ClearWindowsSetupLogs;
            ClearCrashDumpsCheckBox.IsChecked = _settings.ClearCrashDumps;
            ClearPerformanceMonitorDataCheckBox.IsChecked = _settings.ClearPerformanceMonitorData;

            // User Profile Cleanup
            ClearClipboardHistoryCheckBox.IsChecked = _settings.ClearClipboardHistory;
            RebuildSearchIndexCheckBox.IsChecked = _settings.RebuildSearchIndex;
            ClearUserAssistDataCheckBox.IsChecked = _settings.ClearUserAssistData;
            ClearTypedPathsCheckBox.IsChecked = _settings.ClearTypedPaths;
            ClearMUICacheCheckBox.IsChecked = _settings.ClearMUICache;
            ClearRecentAppsCheckBox.IsChecked = _settings.ClearRecentApps;

            // Network Cache
            FlushDNSCacheCheckBox.IsChecked = _settings.FlushDNSCache;
            ClearNetBIOSCacheCheckBox.IsChecked = _settings.ClearNetBIOSCache;
            ClearARPCacheCheckBox.IsChecked = _settings.ClearARPCache;
            ClearWindowsNetworkingCacheCheckBox.IsChecked = _settings.ClearWindowsNetworkingCache;

            // Large System Files
            RemoveWindowsOldCheckBox.IsChecked = _settings.RemoveWindowsOld;
            CleanDriverStoreCheckBox.IsChecked = _settings.CleanDriverStore;
            CleanWindowsInstallerCacheCheckBox.IsChecked = _settings.CleanWindowsInstallerCache;
            DisableHibernationCheckBox.IsChecked = _settings.DisableHibernation;
            CleanSystemRestorePointsCheckBox.IsChecked = _settings.CleanSystemRestorePoints;

            // Registry Optimization
            ClearMRUListsCheckBox.IsChecked = _settings.ClearMRULists;
            CleanFileExtensionAssociationsCheckBox.IsChecked = _settings.CleanFileExtensionAssociations;
            CleanUninstallEntriesCheckBox.IsChecked = _settings.CleanUninstallEntries;
            CleanSharedDLLsCheckBox.IsChecked = _settings.CleanSharedDLLs;
            CleanCOMRegistrationsCheckBox.IsChecked = _settings.CleanCOMRegistrations;
        }

        private void PersistSettings()
        {
            // Storage
            _settings.RemoveJunkFiles = JunkFilesCheckBox.IsChecked ?? false;
            _settings.CleanSystemTemporaryFiles = SystemTempCheckBox.IsChecked ?? false;
            _settings.EmptyRecycleBin = RecycleBinCheckBox.IsChecked ?? false;
            _settings.RemoveDiagnosticsAndErrorReports = DiagnosticsCheckBox.IsChecked ?? false;
            _settings.ClearVisualCache = VisualCacheCheckBox.IsChecked ?? false;

            // History and Privacy
            _settings.ClearFileHistory = FileHistoryCheckBox.IsChecked ?? false;
            _settings.WipeBrowserData = BrowserDataCheckBox.IsChecked ?? false;
            _settings.RemoveWindowsDefenderHistory = WindowsDefenderCheckBox.IsChecked ?? false;

            // System
            _settings.CleanRegistry = RegistryCleanupCheckBox.IsChecked ?? false;
            _settings.CleanComponentStore = ComponentStoreCheckBox.IsChecked ?? false;

            // Clutter
            _settings.RemoveEmptyDirectories = EmptyDirsCheckBox.IsChecked ?? false;
            _settings.RemoveBrokenShortcuts = BrokenShortcutsCheckBox.IsChecked ?? false;

            // Windows System Cache
            _settings.ClearFontCache = ClearFontCacheCheckBox.IsChecked ?? false;
            _settings.ClearWindowsStoreCache = ClearWindowsStoreCacheCheckBox.IsChecked ?? false;
            _settings.CleanWindowsUpdate = CleanWindowsUpdateCheckBox.IsChecked ?? false;
            _settings.ClearNetworkLocationCache = ClearNetworkLocationCacheCheckBox.IsChecked ?? false;
            _settings.ClearBITSQueue = ClearBITSQueueCheckBox.IsChecked ?? false;
            _settings.ClearCBSLogs = ClearCBSLogsCheckBox.IsChecked ?? false;

            // Event & Diagnostic Logs
            _settings.ClearEventLogs = ClearEventLogsCheckBox.IsChecked ?? false;
            _settings.ClearWindowsSetupLogs = ClearWindowsSetupLogsCheckBox.IsChecked ?? false;
            _settings.ClearCrashDumps = ClearCrashDumpsCheckBox.IsChecked ?? false;
            _settings.ClearPerformanceMonitorData = ClearPerformanceMonitorDataCheckBox.IsChecked ?? false;

            // User Profile Cleanup
            _settings.ClearClipboardHistory = ClearClipboardHistoryCheckBox.IsChecked ?? false;
            _settings.RebuildSearchIndex = RebuildSearchIndexCheckBox.IsChecked ?? false;
            _settings.ClearUserAssistData = ClearUserAssistDataCheckBox.IsChecked ?? false;
            _settings.ClearTypedPaths = ClearTypedPathsCheckBox.IsChecked ?? false;
            _settings.ClearMUICache = ClearMUICacheCheckBox.IsChecked ?? false;
            _settings.ClearRecentApps = ClearRecentAppsCheckBox.IsChecked ?? false;

            // Network Cache
            _settings.FlushDNSCache = FlushDNSCacheCheckBox.IsChecked ?? false;
            _settings.ClearNetBIOSCache = ClearNetBIOSCacheCheckBox.IsChecked ?? false;
            _settings.ClearARPCache = ClearARPCacheCheckBox.IsChecked ?? false;
            _settings.ClearWindowsNetworkingCache = ClearWindowsNetworkingCacheCheckBox.IsChecked ?? false;

            // Large System Files
            _settings.RemoveWindowsOld = RemoveWindowsOldCheckBox.IsChecked ?? false;
            _settings.CleanDriverStore = CleanDriverStoreCheckBox.IsChecked ?? false;
            _settings.CleanWindowsInstallerCache = CleanWindowsInstallerCacheCheckBox.IsChecked ?? false;
            _settings.DisableHibernation = DisableHibernationCheckBox.IsChecked ?? false;
            _settings.CleanSystemRestorePoints = CleanSystemRestorePointsCheckBox.IsChecked ?? false;

            // Registry Optimization
            _settings.ClearMRULists = ClearMRUListsCheckBox.IsChecked ?? false;
            _settings.CleanFileExtensionAssociations = CleanFileExtensionAssociationsCheckBox.IsChecked ?? false;
            _settings.CleanUninstallEntries = CleanUninstallEntriesCheckBox.IsChecked ?? false;
            _settings.CleanSharedDLLs = CleanSharedDLLsCheckBox.IsChecked ?? false;
            _settings.CleanCOMRegistrations = CleanCOMRegistrationsCheckBox.IsChecked ?? false;

            _settings.Save();
        }

        private async void CleanButton_Click(object sender, RoutedEventArgs e)
        {
            PersistSettings();

            bool hasSelection = HasAnySelections();

            if (!hasSelection)
            {
                MessageBox.Show("Select at least one cleanup option.", "Clean", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            bool needsElevation = NeedsElevation();
            if (needsElevation && !IsRunningAsAdmin())
            {
                var elevate = MessageBox.Show(
                    "Many selected operations require administrator rights to complete fully. Restart with elevation?",
                    "Administrator required",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (elevate == MessageBoxResult.Yes)
                {
                    RestartElevated();
                    Close();
                    return;
                }
            }

            var confirmation = MessageBox.Show(
                "The selected operations will modify files, caches, and/or the registry. Changes cannot be undone.\n\nContinue?",
                "Confirm cleaning",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (confirmation != MessageBoxResult.Yes)
                return;

            DisableActionButtons(true);

            try
            {
                EnsureProcessesClosed(BrowserDataCheckBox.IsChecked == true);

                string systemDrive = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
                long freeBefore = new DriveInfo(systemDrive).AvailableFreeSpace;

                var tasks = new List<string>();

                // Storage
                if (JunkFilesCheckBox.IsChecked == true)
                {
                    await CleanerOperations.RemoveJunkFilesAsync();
                    tasks.Add("User temp files");
                }
                if (SystemTempCheckBox.IsChecked == true)
                {
                    await CleanerOperations.CleanSystemTemporaryFilesAsync();
                    tasks.Add("System temp/update caches");
                }
                if (RecycleBinCheckBox.IsChecked == true)
                {
                    await CleanerOperations.EmptyRecycleBinAsync();
                    tasks.Add("Recycle bin");
                }
                if (DiagnosticsCheckBox.IsChecked == true)
                {
                    await CleanerOperations.RemoveDiagnosticsAndErrorReportsAsync();
                    tasks.Add("Diagnostics/error reports");
                }
                if (VisualCacheCheckBox.IsChecked == true)
                {
                    await CleanerOperations.ClearVisualCacheAsync();
                    tasks.Add("Icon/thumbnail cache");
                }

                // History and Privacy
                if (FileHistoryCheckBox.IsChecked == true)
                {
                    await CleanerOperations.ClearFileHistoryAsync();
                    tasks.Add("Explorer history");
                }
                if (BrowserDataCheckBox.IsChecked == true)
                {
                    await CleanerOperations.WipeBrowserDataAsync();
                    tasks.Add("Browser data");
                }
                if (WindowsDefenderCheckBox.IsChecked == true)
                {
                    await CleanerOperations.ClearWindowsDefenderHistoryAsync();
                    tasks.Add("Defender history");
                }

                // System
                if (RegistryCleanupCheckBox.IsChecked == true)
                {
                    await CleanerOperations.CleanRegistryAsync();
                    tasks.Add("Registry run entries");
                }
                if (ComponentStoreCheckBox.IsChecked == true)
                {
                    await CleanerOperations.CleanComponentStoreAsync();
                    tasks.Add("Component store");
                }

                // Clutter
                if (EmptyDirsCheckBox.IsChecked == true)
                {
                    await CleanerOperations.RemoveEmptyDirectoriesAsync();
                    tasks.Add("Empty directories");
                }
                if (BrokenShortcutsCheckBox.IsChecked == true)
                {
                    await CleanerOperations.RemoveBrokenShortcutsAsync();
                    tasks.Add("Broken shortcuts");
                }

                // Windows System Cache
                if (ClearFontCacheCheckBox.IsChecked == true)
                {
                    await CleanerOperations.ClearFontCacheAsync();
                    tasks.Add("Font cache");
                }
                if (ClearWindowsStoreCacheCheckBox.IsChecked == true)
                {
                    await CleanerOperations.ClearWindowsStoreCacheAsync();
                    tasks.Add("Store cache");
                }
                if (CleanWindowsUpdateCheckBox.IsChecked == true)
                {
                    await CleanerOperations.CleanWindowsUpdateAsync();
                    tasks.Add("Windows Update");
                }
                if (ClearNetworkLocationCacheCheckBox.IsChecked == true)
                {
                    await CleanerOperations.ClearNetworkLocationCacheAsync();
                    tasks.Add("Network location cache");
                }
                if (ClearBITSQueueCheckBox.IsChecked == true)
                {
                    await CleanerOperations.ClearBITSQueueAsync();
                    tasks.Add("BITS queue");
                }
                if (ClearCBSLogsCheckBox.IsChecked == true)
                {
                    await CleanerOperations.ClearCBSLogsAsync();
                    tasks.Add("CBS logs");
                }

                // Event & Diagnostic Logs
                if (ClearEventLogsCheckBox.IsChecked == true)
                {
                    await CleanerOperations.ClearEventLogsAsync();
                    tasks.Add("Event logs");
                }
                if (ClearWindowsSetupLogsCheckBox.IsChecked == true)
                {
                    await CleanerOperations.ClearWindowsSetupLogsAsync();
                    tasks.Add("Setup logs");
                }
                if (ClearCrashDumpsCheckBox.IsChecked == true)
                {
                    await CleanerOperations.ClearCrashDumpsAsync();
                    tasks.Add("Crash dumps");
                }
                if (ClearPerformanceMonitorDataCheckBox.IsChecked == true)
                {
                    await CleanerOperations.ClearPerformanceMonitorDataAsync();
                    tasks.Add("Performance data");
                }

                // User Profile Cleanup
                if (ClearClipboardHistoryCheckBox.IsChecked == true)
                {
                    await CleanerOperations.ClearClipboardHistoryAsync();
                    tasks.Add("Clipboard history");
                }
                if (RebuildSearchIndexCheckBox.IsChecked == true)
                {
                    await CleanerOperations.RebuildSearchIndexAsync();
                    tasks.Add("Search index");
                }
                if (ClearUserAssistDataCheckBox.IsChecked == true)
                {
                    await CleanerOperations.ClearUserAssistDataAsync();
                    tasks.Add("UserAssist data");
                }
                if (ClearTypedPathsCheckBox.IsChecked == true)
                {
                    await CleanerOperations.ClearTypedPathsAsync();
                    tasks.Add("Typed paths");
                }
                if (ClearMUICacheCheckBox.IsChecked == true)
                {
                    await CleanerOperations.ClearMUICacheAsync();
                    tasks.Add("MUI cache");
                }
                if (ClearRecentAppsCheckBox.IsChecked == true)
                {
                    await CleanerOperations.ClearRecentAppsAsync();
                    tasks.Add("Recent apps");
                }

                // Network Cache
                if (FlushDNSCacheCheckBox.IsChecked == true)
                {
                    await CleanerOperations.FlushDNSCacheAsync();
                    tasks.Add("DNS cache");
                }
                if (ClearNetBIOSCacheCheckBox.IsChecked == true)
                {
                    await CleanerOperations.ClearNetBIOSCacheAsync();
                    tasks.Add("NetBIOS cache");
                }
                if (ClearARPCacheCheckBox.IsChecked == true)
                {
                    await CleanerOperations.ClearARPCacheAsync();
                    tasks.Add("ARP cache");
                }
                if (ClearWindowsNetworkingCacheCheckBox.IsChecked == true)
                {
                    await CleanerOperations.ClearWindowsNetworkingCacheAsync();
                    tasks.Add("Network cache");
                }

                // Large System Files
                if (RemoveWindowsOldCheckBox.IsChecked == true)
                {
                    await CleanerOperations.RemoveWindowsOldAsync();
                    tasks.Add("Windows.old");
                }
                if (CleanDriverStoreCheckBox.IsChecked == true)
                {
                    await CleanerOperations.CleanDriverStoreAsync();
                    tasks.Add("Driver store");
                }
                if (CleanWindowsInstallerCacheCheckBox.IsChecked == true)
                {
                    await CleanerOperations.CleanWindowsInstallerCacheAsync();
                    tasks.Add("Installer cache");
                }
                if (DisableHibernationCheckBox.IsChecked == true)
                {
                    await CleanerOperations.DisableHibernationAsync();
                    tasks.Add("Hibernation");
                }
                if (CleanSystemRestorePointsCheckBox.IsChecked == true)
                {
                    await CleanerOperations.CleanSystemRestorePointsAsync();
                    tasks.Add("Restore points");
                }

                // Registry Optimization
                if (ClearMRUListsCheckBox.IsChecked == true)
                {
                    await CleanerOperations.ClearMRUListsAsync();
                    tasks.Add("MRU lists");
                }
                if (CleanFileExtensionAssociationsCheckBox.IsChecked == true)
                {
                    await CleanerOperations.CleanFileExtensionAssociationsAsync();
                    tasks.Add("File associations");
                }
                if (CleanUninstallEntriesCheckBox.IsChecked == true)
                {
                    await CleanerOperations.CleanUninstallEntriesAsync();
                    tasks.Add("Uninstall entries");
                }
                if (CleanSharedDLLsCheckBox.IsChecked == true)
                {
                    await CleanerOperations.CleanSharedDLLsAsync();
                    tasks.Add("Shared DLLs");
                }
                if (CleanCOMRegistrationsCheckBox.IsChecked == true)
                {
                    await CleanerOperations.CleanCOMRegistrationsAsync();
                    tasks.Add("COM registrations");
                }

                long freeAfter = new DriveInfo(systemDrive).AvailableFreeSpace;
                long reclaimed = Math.Max(0, freeAfter - freeBefore);

                var reportLines = new List<string>
                {
                    $"Tasks completed: {tasks.Count}",
                    $"Space reclaimed: {FormatBytes(reclaimed)}"
                };

                MessageBox.Show(string.Join(Environment.NewLine, reportLines), "Cleaning complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                DisableActionButtons(false);
            }
        }

        private bool HasAnySelections()
        {
            return JunkFilesCheckBox.IsChecked == true ||
                   SystemTempCheckBox.IsChecked == true ||
                   RecycleBinCheckBox.IsChecked == true ||
                   DiagnosticsCheckBox.IsChecked == true ||
                   VisualCacheCheckBox.IsChecked == true ||
                   FileHistoryCheckBox.IsChecked == true ||
                   BrowserDataCheckBox.IsChecked == true ||
                   WindowsDefenderCheckBox.IsChecked == true ||
                   RegistryCleanupCheckBox.IsChecked == true ||
                   ComponentStoreCheckBox.IsChecked == true ||
                   EmptyDirsCheckBox.IsChecked == true ||
                   BrokenShortcutsCheckBox.IsChecked == true ||
                   ClearFontCacheCheckBox.IsChecked == true ||
                   ClearWindowsStoreCacheCheckBox.IsChecked == true ||
                   CleanWindowsUpdateCheckBox.IsChecked == true ||
                   ClearNetworkLocationCacheCheckBox.IsChecked == true ||
                   ClearBITSQueueCheckBox.IsChecked == true ||
                   ClearCBSLogsCheckBox.IsChecked == true ||
                   ClearEventLogsCheckBox.IsChecked == true ||
                   ClearWindowsSetupLogsCheckBox.IsChecked == true ||
                   ClearCrashDumpsCheckBox.IsChecked == true ||
                   ClearPerformanceMonitorDataCheckBox.IsChecked == true ||
                   ClearClipboardHistoryCheckBox.IsChecked == true ||
                   RebuildSearchIndexCheckBox.IsChecked == true ||
                   ClearUserAssistDataCheckBox.IsChecked == true ||
                   ClearTypedPathsCheckBox.IsChecked == true ||
                   ClearMUICacheCheckBox.IsChecked == true ||
                   ClearRecentAppsCheckBox.IsChecked == true ||
                   FlushDNSCacheCheckBox.IsChecked == true ||
                   ClearNetBIOSCacheCheckBox.IsChecked == true ||
                   ClearARPCacheCheckBox.IsChecked == true ||
                   ClearWindowsNetworkingCacheCheckBox.IsChecked == true ||
                   RemoveWindowsOldCheckBox.IsChecked == true ||
                   CleanDriverStoreCheckBox.IsChecked == true ||
                   CleanWindowsInstallerCacheCheckBox.IsChecked == true ||
                   DisableHibernationCheckBox.IsChecked == true ||
                   CleanSystemRestorePointsCheckBox.IsChecked == true ||
                   ClearMRUListsCheckBox.IsChecked == true ||
                   CleanFileExtensionAssociationsCheckBox.IsChecked == true ||
                   CleanUninstallEntriesCheckBox.IsChecked == true ||
                   CleanSharedDLLsCheckBox.IsChecked == true ||
                   CleanCOMRegistrationsCheckBox.IsChecked == true;
        }

        private bool NeedsElevation()
        {
            return SystemTempCheckBox.IsChecked == true ||
                   RecycleBinCheckBox.IsChecked == true ||
                   WindowsDefenderCheckBox.IsChecked == true ||
                   ComponentStoreCheckBox.IsChecked == true ||
                   RegistryCleanupCheckBox.IsChecked == true ||
                   ClearFontCacheCheckBox.IsChecked == true ||
                   CleanWindowsUpdateCheckBox.IsChecked == true ||
                   ClearNetworkLocationCacheCheckBox.IsChecked == true ||
                   ClearEventLogsCheckBox.IsChecked == true ||
                   ClearWindowsSetupLogsCheckBox.IsChecked == true ||
                   ClearCrashDumpsCheckBox.IsChecked == true ||
                   RebuildSearchIndexCheckBox.IsChecked == true ||
                   RemoveWindowsOldCheckBox.IsChecked == true ||
                   CleanDriverStoreCheckBox.IsChecked == true ||
                   CleanWindowsInstallerCacheCheckBox.IsChecked == true ||
                   DisableHibernationCheckBox.IsChecked == true ||
                   CleanSystemRestorePointsCheckBox.IsChecked == true ||
                   CleanUninstallEntriesCheckBox.IsChecked == true ||
                   CleanSharedDLLsCheckBox.IsChecked == true ||
                   CleanCOMRegistrationsCheckBox.IsChecked == true;
        }

        private static bool IsRunningAsAdmin()
        {
            try
            {
                using WindowsIdentity identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        private void RestartElevated()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = Environment.ProcessPath ?? "SystemCleaner.exe",
                    UseShellExecute = true,
                    Verb = "runas"
                };
                Process.Start(psi);
            }
            catch
            {
                // ignore failures
            }
        }

        private void EnsureProcessesClosed(bool browsersSelected)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (browsersSelected)
            {
                names.UnionWith(new[] { "chrome", "msedge", "brave", "vivaldi", "opera", "arc", "firefox", "waterfox", "palemoon" });
            }

            if (!names.Any()) return;

            var running = Process.GetProcesses()
                .Where(p => names.Contains(p.ProcessName))
                .ToList();

            if (!running.Any()) return;

            var list = string.Join(", ", running.Select(p => p.ProcessName).Distinct());
            var close = MessageBox.Show(
                $"These apps are running and may lock files: {list}\n\nClose them now?",
                "Close apps",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (close != MessageBoxResult.Yes) return;

            foreach (var proc in running)
            {
                try
                {
                    proc.CloseMainWindow();
                    proc.WaitForExit(4000);
                }
                catch { }
            }
        }

        private void DisableActionButtons(bool isDisabled)
        {
            CleanButton.IsEnabled = !isDisabled;
        }

        private static string FormatBytes(long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            double value = bytes;
            int unit = 0;
            while (value >= 1024 && unit < units.Length - 1)
            {
                value /= 1024;
                unit++;
            }
            return $"{value:0.##} {units[unit]}";
        }
    }
}
