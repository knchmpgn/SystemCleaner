using System;
using System.IO;
using System.Text.Json;

namespace SystemCleaner.Models
{
    /// <summary>
    /// Represents persisted user preferences for cleaning operations.
    /// </summary>
    public class CleanerSettings
    {
        // Storage
        public bool RemoveJunkFiles { get; set; } = false;
        public bool CleanSystemTemporaryFiles { get; set; } = false;
        public bool EmptyRecycleBin { get; set; } = false;
        public bool RemoveDiagnosticsAndErrorReports { get; set; } = false;
        public bool ClearVisualCache { get; set; } = false;

        // History and Privacy
        public bool ClearFileHistory { get; set; } = false;
        public bool WipeBrowserData { get; set; } = false;
        public bool RemoveWindowsDefenderHistory { get; set; } = false;

        // System
        public bool CleanRegistry { get; set; } = false;
        public bool CleanComponentStore { get; set; } = false;

        // Clutter
        public bool RemoveEmptyDirectories { get; set; } = false;
        public bool RemoveBrokenShortcuts { get; set; } = false;

        // Windows System Cache
        public bool ClearFontCache { get; set; } = false;
        public bool ClearWindowsStoreCache { get; set; } = false;
        public bool CleanWindowsUpdate { get; set; } = false;
        public bool ClearNetworkLocationCache { get; set; } = false;
        public bool ClearBITSQueue { get; set; } = false;
        public bool ClearCBSLogs { get; set; } = false;

        // Event & Diagnostic Logs
        public bool ClearEventLogs { get; set; } = false;
        public bool ClearWindowsSetupLogs { get; set; } = false;
        public bool ClearCrashDumps { get; set; } = false;
        public bool ClearPerformanceMonitorData { get; set; } = false;

        // User Profile Cleanup
        public bool ClearClipboardHistory { get; set; } = false;
        public bool RebuildSearchIndex { get; set; } = false;
        public bool ClearUserAssistData { get; set; } = false;
        public bool ClearTypedPaths { get; set; } = false;
        public bool ClearMUICache { get; set; } = false;
        public bool ClearRecentApps { get; set; } = false;

        // Network Cache
        public bool FlushDNSCache { get; set; } = false;
        public bool ClearNetBIOSCache { get; set; } = false;
        public bool ClearARPCache { get; set; } = false;
        public bool ClearWindowsNetworkingCache { get; set; } = false;

        // Large System Files
        public bool RemoveWindowsOld { get; set; } = false;
        public bool CleanDriverStore { get; set; } = false;
        public bool CleanWindowsInstallerCache { get; set; } = false;
        public bool DisableHibernation { get; set; } = false;
        public bool CleanSystemRestorePoints { get; set; } = false;

        // Registry Optimization
        public bool ClearMRULists { get; set; } = false;
        public bool CleanFileExtensionAssociations { get; set; } = false;
        public bool CleanUninstallEntries { get; set; } = false;
        public bool CleanSharedDLLs { get; set; } = false;
        public bool CleanCOMRegistrations { get; set; } = false;

        private static readonly string ConfigFileName = "settings.json";

        /// <summary>
        /// Loads settings from disk if a configuration file is available.
        /// Otherwise returns default settings.
        /// </summary>
        public static CleanerSettings Load(string? folderPath = null)
        {
            try
            {
                string basePath = folderPath ?? AppDomain.CurrentDomain.BaseDirectory;
                string configPath = Path.Combine(basePath, ConfigFileName);
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    var settings = JsonSerializer.Deserialize<CleanerSettings>(json);
                    if (settings != null)
                    {
                        return settings;
                    }
                }
            }
            catch
            {
                // ignore and fall through to defaults
            }
            return new CleanerSettings();
        }

        /// <summary>
        /// Saves the settings to disk.
        /// </summary>
        public void Save(string? folderPath = null)
        {
            try
            {
                string basePath = folderPath ?? AppDomain.CurrentDomain.BaseDirectory;
                string configPath = Path.Combine(basePath, ConfigFileName);
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, json);
            }
            catch
            {
                // ignore; saving settings is best effort
            }
        }
    }
}
