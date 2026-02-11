using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        private int _totalTasks;
        private int _completedTasks;

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
            // Quick Cleanup
            JunkFilesCheckBox.IsChecked = _settings.RemoveJunkFiles;
            SystemTempCheckBox.IsChecked = _settings.CleanSystemTemporaryFiles;
            RecycleBinCheckBox.IsChecked = _settings.EmptyRecycleBin;
            BrowserDataCheckBox.IsChecked = _settings.WipeBrowserData;
            FileHistoryCheckBox.IsChecked = _settings.ClearFileHistory;

            // Privacy & Tracking
            WindowsDefenderCheckBox.IsChecked = _settings.RemoveWindowsDefenderHistory;
            ClearUserAssistDataCheckBox.IsChecked = _settings.ClearUserAssistData;
            ClearTypedPathsCheckBox.IsChecked = _settings.ClearTypedPaths;
            ClearRecentAppsCheckBox.IsChecked = _settings.ClearRecentApps;
            ClearClipboardHistoryCheckBox.IsChecked = _settings.ClearClipboardHistory;
            ClearMRUListsCheckBox.IsChecked = _settings.ClearMRULists;

            // System Maintenance
            VisualCacheCheckBox.IsChecked = _settings.ClearVisualCache;
            ClearFontCacheCheckBox.IsChecked = _settings.ClearFontCache;
            ClearWindowsStoreCacheCheckBox.IsChecked = _settings.ClearWindowsStoreCache;
            ComponentStoreCheckBox.IsChecked = _settings.CleanComponentStore;
            CleanWindowsUpdateCheckBox.IsChecked = _settings.CleanWindowsUpdate;

            // Logs & Diagnostics
            DiagnosticsCheckBox.IsChecked = _settings.RemoveDiagnosticsAndErrorReports;
            ClearEventLogsCheckBox.IsChecked = _settings.ClearEventLogs;
            ClearWindowsSetupLogsCheckBox.IsChecked = _settings.ClearWindowsSetupLogs;
            ClearCrashDumpsCheckBox.IsChecked = _settings.ClearCrashDumps;
            ClearPerformanceMonitorDataCheckBox.IsChecked = _settings.ClearPerformanceMonitorData;
            ClearCBSLogsCheckBox.IsChecked = _settings.ClearCBSLogs;

            // Network
            FlushDNSCacheCheckBox.IsChecked = _settings.FlushDNSCache;
            ClearNetBIOSCacheCheckBox.IsChecked = _settings.ClearNetBIOSCache;
            ClearARPCacheCheckBox.IsChecked = _settings.ClearARPCache;
            ClearWindowsNetworkingCacheCheckBox.IsChecked = _settings.ClearWindowsNetworkingCache;
            ClearNetworkLocationCacheCheckBox.IsChecked = _settings.ClearNetworkLocationCache;
            ClearBITSQueueCheckBox.IsChecked = _settings.ClearBITSQueue;

            // Registry
            RegistryCleanupCheckBox.IsChecked = _settings.CleanRegistry;
            CleanFileExtensionAssociationsCheckBox.IsChecked = _settings.CleanFileExtensionAssociations;
            CleanUninstallEntriesCheckBox.IsChecked = _settings.CleanUninstallEntries;
            CleanSharedDLLsCheckBox.IsChecked = _settings.CleanSharedDLLs;
            CleanCOMRegistrationsCheckBox.IsChecked = _settings.CleanCOMRegistrations;
            ClearMUICacheCheckBox.IsChecked = _settings.ClearMUICache;

            // Advanced
            EmptyDirsCheckBox.IsChecked = _settings.RemoveEmptyDirectories;
            BrokenShortcutsCheckBox.IsChecked = _settings.RemoveBrokenShortcuts;
            RemoveWindowsOldCheckBox.IsChecked = _settings.RemoveWindowsOld;
            CleanDriverStoreCheckBox.IsChecked = _settings.CleanDriverStore;
            CleanWindowsInstallerCacheCheckBox.IsChecked = _settings.CleanWindowsInstallerCache;
            DisableHibernationCheckBox.IsChecked = _settings.DisableHibernation;
            CleanSystemRestorePointsCheckBox.IsChecked = _settings.CleanSystemRestorePoints;
            RebuildSearchIndexCheckBox.IsChecked = _settings.RebuildSearchIndex;
        }

        private void PersistSettings()
        {
            // Quick Cleanup
            _settings.RemoveJunkFiles = JunkFilesCheckBox.IsChecked ?? false;
            _settings.CleanSystemTemporaryFiles = SystemTempCheckBox.IsChecked ?? false;
            _settings.EmptyRecycleBin = RecycleBinCheckBox.IsChecked ?? false;
            _settings.WipeBrowserData = BrowserDataCheckBox.IsChecked ?? false;
            _settings.ClearFileHistory = FileHistoryCheckBox.IsChecked ?? false;

            // Privacy & Tracking
            _settings.RemoveWindowsDefenderHistory = WindowsDefenderCheckBox.IsChecked ?? false;
            _settings.ClearUserAssistData = ClearUserAssistDataCheckBox.IsChecked ?? false;
            _settings.ClearTypedPaths = ClearTypedPathsCheckBox.IsChecked ?? false;
            _settings.ClearRecentApps = ClearRecentAppsCheckBox.IsChecked ?? false;
            _settings.ClearClipboardHistory = ClearClipboardHistoryCheckBox.IsChecked ?? false;
            _settings.ClearMRULists = ClearMRUListsCheckBox.IsChecked ?? false;

            // System Maintenance
            _settings.ClearVisualCache = VisualCacheCheckBox.IsChecked ?? false;
            _settings.ClearFontCache = ClearFontCacheCheckBox.IsChecked ?? false;
            _settings.ClearWindowsStoreCache = ClearWindowsStoreCacheCheckBox.IsChecked ?? false;
            _settings.CleanComponentStore = ComponentStoreCheckBox.IsChecked ?? false;
            _settings.CleanWindowsUpdate = CleanWindowsUpdateCheckBox.IsChecked ?? false;

            // Logs & Diagnostics
            _settings.RemoveDiagnosticsAndErrorReports = DiagnosticsCheckBox.IsChecked ?? false;
            _settings.ClearEventLogs = ClearEventLogsCheckBox.IsChecked ?? false;
            _settings.ClearWindowsSetupLogs = ClearWindowsSetupLogsCheckBox.IsChecked ?? false;
            _settings.ClearCrashDumps = ClearCrashDumpsCheckBox.IsChecked ?? false;
            _settings.ClearPerformanceMonitorData = ClearPerformanceMonitorDataCheckBox.IsChecked ?? false;
            _settings.ClearCBSLogs = ClearCBSLogsCheckBox.IsChecked ?? false;

            // Network
            _settings.FlushDNSCache = FlushDNSCacheCheckBox.IsChecked ?? false;
            _settings.ClearNetBIOSCache = ClearNetBIOSCacheCheckBox.IsChecked ?? false;
            _settings.ClearARPCache = ClearARPCacheCheckBox.IsChecked ?? false;
            _settings.ClearWindowsNetworkingCache = ClearWindowsNetworkingCacheCheckBox.IsChecked ?? false;
            _settings.ClearNetworkLocationCache = ClearNetworkLocationCacheCheckBox.IsChecked ?? false;
            _settings.ClearBITSQueue = ClearBITSQueueCheckBox.IsChecked ?? false;

            // Registry
            _settings.CleanRegistry = RegistryCleanupCheckBox.IsChecked ?? false;
            _settings.CleanFileExtensionAssociations = CleanFileExtensionAssociationsCheckBox.IsChecked ?? false;
            _settings.CleanUninstallEntries = CleanUninstallEntriesCheckBox.IsChecked ?? false;
            _settings.CleanSharedDLLs = CleanSharedDLLsCheckBox.IsChecked ?? false;
            _settings.CleanCOMRegistrations = CleanCOMRegistrationsCheckBox.IsChecked ?? false;
            _settings.ClearMUICache = ClearMUICacheCheckBox.IsChecked ?? false;

            // Advanced
            _settings.RemoveEmptyDirectories = EmptyDirsCheckBox.IsChecked ?? false;
            _settings.RemoveBrokenShortcuts = BrokenShortcutsCheckBox.IsChecked ?? false;
            _settings.RemoveWindowsOld = RemoveWindowsOldCheckBox.IsChecked ?? false;
            _settings.CleanDriverStore = CleanDriverStoreCheckBox.IsChecked ?? false;
            _settings.CleanWindowsInstallerCache = CleanWindowsInstallerCacheCheckBox.IsChecked ?? false;
            _settings.DisableHibernation = DisableHibernationCheckBox.IsChecked ?? false;
            _settings.CleanSystemRestorePoints = CleanSystemRestorePointsCheckBox.IsChecked ?? false;
            _settings.RebuildSearchIndex = RebuildSearchIndexCheckBox.IsChecked ?? false;

            _settings.Save();
        }

        private async void CleanButton_Click(object sender, RoutedEventArgs e)
        {
            PersistSettings();

            bool hasSelection = HasAnySelections();

            if (!hasSelection)
            {
                CustomDialog.Show("Select at least one cleanup option.", "Clean", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirmation = CustomDialog.Show(
                "The selected operations will modify files, caches, and/or the registry. Changes cannot be undone.\n\nContinue?",
                "Confirm cleaning",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (confirmation != MessageBoxResult.Yes)
                return;

            DisableActionButtons(true);
            ShowProgress(true);

            try
            {
                EnsureProcessesClosed(BrowserDataCheckBox.IsChecked == true);

                string systemDrive = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
                long freeBefore = new DriveInfo(systemDrive).AvailableFreeSpace;

                var tasks = new List<string>();
                _totalTasks = CountSelectedTasks();
                _completedTasks = 0;

                // Quick Cleanup
                if (JunkFilesCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Removing user temp files...", CleanerOperations.RemoveJunkFilesAsync());
                    tasks.Add("User temp files");
                }
                if (SystemTempCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Cleaning system temp/update caches...", CleanerOperations.CleanSystemTemporaryFilesAsync());
                    tasks.Add("System temp/update caches");
                }
                if (RecycleBinCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Emptying recycle bin...", CleanerOperations.EmptyRecycleBinAsync());
                    tasks.Add("Recycle bin");
                }
                if (BrowserDataCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Wiping browser data...", CleanerOperations.WipeBrowserDataAsync());
                    tasks.Add("Browser data");
                }
                if (FileHistoryCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Clearing Explorer history...", CleanerOperations.ClearFileHistoryAsync());
                    tasks.Add("Explorer history");
                }

                // Privacy & Tracking
                if (WindowsDefenderCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Clearing Defender history...", CleanerOperations.ClearWindowsDefenderHistoryAsync());
                    tasks.Add("Defender history");
                }
                if (ClearUserAssistDataCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Clearing UserAssist data...", CleanerOperations.ClearUserAssistDataAsync());
                    tasks.Add("UserAssist data");
                }
                if (ClearTypedPathsCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Clearing typed paths...", CleanerOperations.ClearTypedPathsAsync());
                    tasks.Add("Typed paths");
                }
                if (ClearRecentAppsCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Clearing recent apps...", CleanerOperations.ClearRecentAppsAsync());
                    tasks.Add("Recent apps");
                }
                if (ClearClipboardHistoryCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Clearing clipboard history...", CleanerOperations.ClearClipboardHistoryAsync());
                    tasks.Add("Clipboard history");
                }
                if (ClearMRUListsCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Clearing MRU lists...", CleanerOperations.ClearMRUListsAsync());
                    tasks.Add("MRU lists");
                }

                // System Maintenance
                if (VisualCacheCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Clearing icon/thumbnail cache...", CleanerOperations.ClearVisualCacheAsync());
                    tasks.Add("Icon/thumbnail cache");
                }
                if (ClearFontCacheCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Clearing font cache...", CleanerOperations.ClearFontCacheAsync());
                    tasks.Add("Font cache");
                }
                if (ClearWindowsStoreCacheCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Clearing Store cache...", CleanerOperations.ClearWindowsStoreCacheAsync());
                    tasks.Add("Store cache");
                }
                if (ComponentStoreCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Cleaning component store...", CleanerOperations.CleanComponentStoreAsync());
                    tasks.Add("Component store");
                }
                if (CleanWindowsUpdateCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Cleaning Windows Update...", CleanerOperations.CleanWindowsUpdateAsync());
                    tasks.Add("Windows Update");
                }

                // Logs & Diagnostics
                if (DiagnosticsCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Removing diagnostics/error reports...", CleanerOperations.RemoveDiagnosticsAndErrorReportsAsync());
                    tasks.Add("Diagnostics/error reports");
                }
                if (ClearEventLogsCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Clearing event logs...", CleanerOperations.ClearEventLogsAsync());
                    tasks.Add("Event logs");
                }
                if (ClearWindowsSetupLogsCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Clearing setup logs...", CleanerOperations.ClearWindowsSetupLogsAsync());
                    tasks.Add("Setup logs");
                }
                if (ClearCrashDumpsCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Clearing crash dumps...", CleanerOperations.ClearCrashDumpsAsync());
                    tasks.Add("Crash dumps");
                }
                if (ClearPerformanceMonitorDataCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Clearing performance data...", CleanerOperations.ClearPerformanceMonitorDataAsync());
                    tasks.Add("Performance data");
                }
                if (ClearCBSLogsCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Clearing CBS logs...", CleanerOperations.ClearCBSLogsAsync());
                    tasks.Add("CBS logs");
                }

                // Network
                if (FlushDNSCacheCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Flushing DNS cache...", CleanerOperations.FlushDNSCacheAsync());
                    tasks.Add("DNS cache");
                }
                if (ClearNetBIOSCacheCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Clearing NetBIOS cache...", CleanerOperations.ClearNetBIOSCacheAsync());
                    tasks.Add("NetBIOS cache");
                }
                if (ClearARPCacheCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Clearing ARP cache...", CleanerOperations.ClearARPCacheAsync());
                    tasks.Add("ARP cache");
                }
                if (ClearWindowsNetworkingCacheCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Clearing network cache...", CleanerOperations.ClearWindowsNetworkingCacheAsync());
                    tasks.Add("Network cache");
                }
                if (ClearNetworkLocationCacheCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Clearing network location cache...", CleanerOperations.ClearNetworkLocationCacheAsync());
                    tasks.Add("Network location cache");
                }
                if (ClearBITSQueueCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Clearing BITS queue...", CleanerOperations.ClearBITSQueueAsync());
                    tasks.Add("BITS queue");
                }

                // Registry
                if (RegistryCleanupCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Cleaning registry run entries...", CleanerOperations.CleanRegistryAsync());
                    tasks.Add("Registry run entries");
                }
                if (CleanFileExtensionAssociationsCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Cleaning file associations...", CleanerOperations.CleanFileExtensionAssociationsAsync());
                    tasks.Add("File associations");
                }
                if (CleanUninstallEntriesCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Cleaning uninstall entries...", CleanerOperations.CleanUninstallEntriesAsync());
                    tasks.Add("Uninstall entries");
                }
                if (CleanSharedDLLsCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Cleaning shared DLLs...", CleanerOperations.CleanSharedDLLsAsync());
                    tasks.Add("Shared DLLs");
                }
                if (CleanCOMRegistrationsCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Cleaning COM registrations...", CleanerOperations.CleanCOMRegistrationsAsync());
                    tasks.Add("COM registrations");
                }
                if (ClearMUICacheCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Clearing MUI cache...", CleanerOperations.ClearMUICacheAsync());
                    tasks.Add("MUI cache");
                }

                // Advanced
                if (EmptyDirsCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Removing empty directories...", CleanerOperations.RemoveEmptyDirectoriesAsync());
                    tasks.Add("Empty directories");
                }
                if (BrokenShortcutsCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Removing broken shortcuts...", CleanerOperations.RemoveBrokenShortcutsAsync());
                    tasks.Add("Broken shortcuts");
                }
                if (RemoveWindowsOldCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Removing Windows.old...", CleanerOperations.RemoveWindowsOldAsync());
                    tasks.Add("Windows.old");
                }
                if (CleanDriverStoreCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Cleaning driver store...", CleanerOperations.CleanDriverStoreAsync());
                    tasks.Add("Driver store");
                }
                if (CleanWindowsInstallerCacheCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Cleaning installer cache...", CleanerOperations.CleanWindowsInstallerCacheAsync());
                    tasks.Add("Installer cache");
                }
                if (DisableHibernationCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Disabling hibernation...", CleanerOperations.DisableHibernationAsync());
                    tasks.Add("Hibernation");
                }
                if (CleanSystemRestorePointsCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Cleaning restore points...", CleanerOperations.CleanSystemRestorePointsAsync());
                    tasks.Add("Restore points");
                }
                if (RebuildSearchIndexCheckBox.IsChecked == true)
                {
                    await RunTaskWithProgress("Rebuilding search index...", CleanerOperations.RebuildSearchIndexAsync());
                    tasks.Add("Search index");
                }

                long freeAfter = new DriveInfo(systemDrive).AvailableFreeSpace;
                long reclaimed = Math.Max(0, freeAfter - freeBefore);

                var reportLines = new List<string>
                {
                    $"Tasks completed: {tasks.Count}",
                    $"Space reclaimed: {FormatBytes(reclaimed)}"
                };

                CustomDialog.Show(string.Join(Environment.NewLine, reportLines), "Cleaning complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                CustomDialog.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ShowProgress(false);
                DisableActionButtons(false);
            }
        }

        private async Task RunTaskWithProgress(string taskName, Task task)
        {
            UpdateProgress(taskName);
            await task;
            _completedTasks++;
            UpdateProgressBar();
        }

        private void UpdateProgress(string taskName)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressTextBlock.Text = taskName;
            });
        }

        private void UpdateProgressBar()
        {
            Dispatcher.Invoke(() =>
            {
                double percentage = (_completedTasks / (double)_totalTasks) * 100;
                CleaningProgressBar.Value = percentage;
            });
        }

        private void ShowProgress(bool show)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressPanel.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
                if (!show)
                {
                    CleaningProgressBar.Value = 0;
                    ProgressTextBlock.Text = "Initializing...";
                }
            });
        }

        private int CountSelectedTasks()
        {
            int count = 0;
            
            // Quick Cleanup
            if (JunkFilesCheckBox.IsChecked == true) count++;
            if (SystemTempCheckBox.IsChecked == true) count++;
            if (RecycleBinCheckBox.IsChecked == true) count++;
            if (BrowserDataCheckBox.IsChecked == true) count++;
            if (FileHistoryCheckBox.IsChecked == true) count++;

            // Privacy & Tracking
            if (WindowsDefenderCheckBox.IsChecked == true) count++;
            if (ClearUserAssistDataCheckBox.IsChecked == true) count++;
            if (ClearTypedPathsCheckBox.IsChecked == true) count++;
            if (ClearRecentAppsCheckBox.IsChecked == true) count++;
            if (ClearClipboardHistoryCheckBox.IsChecked == true) count++;
            if (ClearMRUListsCheckBox.IsChecked == true) count++;

            // System Maintenance
            if (VisualCacheCheckBox.IsChecked == true) count++;
            if (ClearFontCacheCheckBox.IsChecked == true) count++;
            if (ClearWindowsStoreCacheCheckBox.IsChecked == true) count++;
            if (ComponentStoreCheckBox.IsChecked == true) count++;
            if (CleanWindowsUpdateCheckBox.IsChecked == true) count++;

            // Logs & Diagnostics
            if (DiagnosticsCheckBox.IsChecked == true) count++;
            if (ClearEventLogsCheckBox.IsChecked == true) count++;
            if (ClearWindowsSetupLogsCheckBox.IsChecked == true) count++;
            if (ClearCrashDumpsCheckBox.IsChecked == true) count++;
            if (ClearPerformanceMonitorDataCheckBox.IsChecked == true) count++;
            if (ClearCBSLogsCheckBox.IsChecked == true) count++;

            // Network
            if (FlushDNSCacheCheckBox.IsChecked == true) count++;
            if (ClearNetBIOSCacheCheckBox.IsChecked == true) count++;
            if (ClearARPCacheCheckBox.IsChecked == true) count++;
            if (ClearWindowsNetworkingCacheCheckBox.IsChecked == true) count++;
            if (ClearNetworkLocationCacheCheckBox.IsChecked == true) count++;
            if (ClearBITSQueueCheckBox.IsChecked == true) count++;

            // Registry
            if (RegistryCleanupCheckBox.IsChecked == true) count++;
            if (CleanFileExtensionAssociationsCheckBox.IsChecked == true) count++;
            if (CleanUninstallEntriesCheckBox.IsChecked == true) count++;
            if (CleanSharedDLLsCheckBox.IsChecked == true) count++;
            if (CleanCOMRegistrationsCheckBox.IsChecked == true) count++;
            if (ClearMUICacheCheckBox.IsChecked == true) count++;

            // Advanced
            if (EmptyDirsCheckBox.IsChecked == true) count++;
            if (BrokenShortcutsCheckBox.IsChecked == true) count++;
            if (RemoveWindowsOldCheckBox.IsChecked == true) count++;
            if (CleanDriverStoreCheckBox.IsChecked == true) count++;
            if (CleanWindowsInstallerCacheCheckBox.IsChecked == true) count++;
            if (DisableHibernationCheckBox.IsChecked == true) count++;
            if (CleanSystemRestorePointsCheckBox.IsChecked == true) count++;
            if (RebuildSearchIndexCheckBox.IsChecked == true) count++;

            return count;
        }

        private bool HasAnySelections()
        {
            return CountSelectedTasks() > 0;
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
            var close = CustomDialog.Show(
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
