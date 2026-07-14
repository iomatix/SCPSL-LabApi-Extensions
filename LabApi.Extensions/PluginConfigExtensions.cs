using LabApi.Loader;
using LabApi.Loader.Features.Configuration;
using LabApi.Loader.Features.Plugins;
using System;

namespace LabApi.Extensions
{
    /// <summary>
    /// Highly optimized utility extensions for loading and validating plugin sub‑config files safely.
    /// </summary>
    public static class PluginConfigExtensions
    {
        /// <summary>
        /// Loads a sub‑config file or creates a new one if missing or invalid.
        /// Runs optional validation and saves the result back to disk.
        /// </summary>
        /// <typeparam name="TMainConfig">Main plugin config type.</typeparam>
        /// <typeparam name="TSubConfig">Sub‑config type to load.</typeparam>
        /// <param name="plugin">Plugin instance.</param>
        /// <param name="fileName">File name on disk (e.g. "settings.yml").</param>
        /// <param name="validationAction">Optional validation callback.</param>
        /// <returns>Loaded or newly created sub‑config instance.</returns>
        public static TSubConfig LoadOrCreateSubConfig<TMainConfig, TSubConfig>(
            this Plugin<TMainConfig> plugin,
            string fileName,
            Action<TSubConfig> validationAction = null)
            where TMainConfig : LabApiConfig, new()
            where TSubConfig : class, new()
        {
            // FIX: Maintained codebase-wide unified null-checking standard ('== null' instead of 'is null').
            if (plugin == null || string.IsNullOrEmpty(fileName))
                return new TSubConfig();

            // FIX: Simplified block structure. Attempt to load and fallback to new instantiation in one clean statement.
            if (!plugin.TryLoadConfig(fileName, out TSubConfig finalConfig) || finalConfig == null)
            {
                finalConfig = new TSubConfig();
            }

            // Run optional schema verification or default values assignment.
            validationAction?.Invoke(finalConfig);

            // Immediately save it back to make sure newly created files or validated/repaired fields are written to disk.
            plugin.TrySaveConfig(finalConfig, fileName);

            return finalConfig;
        }
    }
}