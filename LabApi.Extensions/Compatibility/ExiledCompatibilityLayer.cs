namespace LabApi.Extensions.Compatibility
{
    using LabApi.Loader;
    using LabApi.Loader.Features.Plugins;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using Logger = LabApi.Extensions.Misc.iLogger;

    /// <summary>
    /// Advanced cross-platform compatibility engine that bridges legacy EXILED configurations 
    /// with the LabAPI architecture using native OS reparse points or runtime mirroring fail-safes.
    /// </summary>
    public static class ExiledCompatibilityLayer
    {
        private const int ProcessTimeoutMilliseconds = 5000;
        private const string LogTag = nameof(ExiledCompatibilityLayer);

        /// <summary>
        /// Executes the synchronization fallback to bridge directories and load missing configurations.
        /// </summary>
        public static void ExecuteFallback(Plugin plugin)
        {
            if (plugin is null) return;

            Logger.Warn(LogTag, "LoadConfigs was bypassed by the native lifecycle. Deploying dynamic cross-framework routing matrix.");

            TryLinkEnvironments(plugin);
            plugin.LoadConfigs();
        }

        private static void TryLinkEnvironments(Plugin plugin)
        {
            try
            {
                // 1. Resolve absolute LabAPI destination path (Port-specific, dynamic, explicit)
                string labApiConfigDir = plugin.GetConfigDirectory(isGlobal: false).FullName;

                if (!Directory.Exists(labApiConfigDir))
                {
                    Directory.CreateDirectory(labApiConfigDir);
                }

                // 2. Extract current server runtime port safely
                string activeServerPort = ServerStatic.ServerPort.ToString();

                // 3. Resolve Cross-Platform AppData baseline (Handles Linux $HOME/.config vs Windows %APPDATA%)
                string baseAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (string.IsNullOrEmpty(baseAppData))
                {
                    // Fallback for isolated Linux Docker environments
                    baseAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
                }

                // 4. Evaluate precise EXILED workspace structures (Port-isolated vs Unified Plugins Folder)
                string exiledPortSpecificPath = Path.Combine(baseAppData, "EXILED", "Configs", activeServerPort, plugin.Name);
                string exiledGlobalPluginsPath = Path.Combine(baseAppData, "EXILED", "Configs", "Plugins", plugin.Name);

                string baseExiledConfigTree = Path.Combine(baseAppData, "EXILED", "Configs", activeServerPort);
                string targetExiledPath = Directory.Exists(baseExiledConfigTree) ? exiledPortSpecificPath : exiledGlobalPluginsPath;

                // 5. Reparse Point Shield: If link already deployed by OS, exit thread immediately
                if (Directory.Exists(targetExiledPath) && IsReparsePoint(targetExiledPath))
                {
                    return;
                }

                // 6. Data Preservation Matrix: If physical folder exists, migrate data before binding link
                if (Directory.Exists(targetExiledPath))
                {
                    Logger.Info(LogTag, "Legacy EXILED directory detected. Migrating historical configuration assets to LabAPI workspace...");
                    MigrateConfigurationFiles(targetExiledPath, labApiConfigDir);

                    Directory.Delete(targetExiledPath, recursive: true);
                }

                // Ensure parent structural tree exists for legacy framework visibility
                string parentDir = Path.GetDirectoryName(targetExiledPath);
                if (!string.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir))
                {
                    Directory.CreateDirectory(parentDir);
                }

                // 7. Execution Loop: Attempt to link environments natively via OS kernel
                if (TryCreateOSLink(targetExiledPath, labApiConfigDir))
                {
                    Logger.Info(LogTag, $"Seamless cross-framework link deployed. [EXILED Proxy]: {targetExiledPath} ===> [LabAPI Workspace]: {labApiConfigDir}");
                }
                else
                {
                    // 8. Silent Fallback Matrix: If OS denies link creation or times out, fall back to passive replication
                    Logger.Warn(LogTag, "OS denied native symlink privileges or execution timed out. Deploying passive replication fallback layer.");
                    Directory.CreateDirectory(targetExiledPath);
                    MigrateConfigurationFiles(labApiConfigDir, targetExiledPath);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(LogTag, $"Critical failure in compatibility matrix routing loop: {ex.Message}");
            }
        }

        private static bool TryCreateOSLink(string linkPath, string targetPath)
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            ProcessStartInfo psi = isWindows
                ? new ProcessStartInfo("cmd.exe", $"/c mklink /J \"{linkPath}\" \"{targetPath}\"")
                : new ProcessStartInfo("/bin/bash", $"-c \"ln -s '{targetPath}' '{linkPath}'\"");

            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.WindowStyle = ProcessWindowStyle.Hidden;

            try
            {
                using Process process = Process.Start(psi);
                if (process is null) return false;

                // Thread-safety Guard: Enforce sub-process execution timeout boundaries to mitigate permanent I/O locks
                if (!process.WaitForExit(ProcessTimeoutMilliseconds))
                {
                    process.Kill();
                    return false;
                }

                return Directory.Exists(linkPath);
            }
            catch
            {
                return false; // Captured failure state gracefully, triggers passive fallback matrix upstream
            }
        }

        private static void MigrateConfigurationFiles(string sourceFolder, string destinationFolder)
        {
            try
            {
                // Performance Optimization: Replaced GetFiles with EnumerateFiles to stream directory indices without allocating arrays
                foreach (string file in Directory.EnumerateFiles(sourceFolder, "*.yml"))
                {
                    string name = Path.GetFileName(file);
                    string dest = Path.Combine(destinationFolder, name);
                    if (!File.Exists(dest))
                    {
                        File.Copy(file, dest, overwrite: true);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(LogTag, $"Asset replication execution choked: {ex.Message}");
            }
        }

        private static bool IsReparsePoint(string path)
        {
            try
            {
                return (File.GetAttributes(path) & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
            }
            catch
            {
                return false;
            }
        }
    }
}