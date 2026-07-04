using System;
using System.Linq;
using System.Reflection;

namespace LabApi.Extensions
{
    /// <summary>
    /// Reflection and manifest tracking utilities for managing embedded assembly assets fluently.
    /// </summary>
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Dynamically resolves an embedded manifest resource pathway matching primary identifiers or structural fallback names.
        /// </summary>
        /// <param name="assembly">The targeting assembly domain containing the embedded assets.</param>
        /// <param name="primaryKey">The core identifier string tracking the asset.</param>
        /// <param name="fileExtension">The target file extension including the leading dot (e.g., '.wav'). Defaults to '.wav'.</param>
        /// <param name="alternativeTokens">Optional fallback identifiers or enum-based naming keys to validate against.</param>
        /// <returns>The fully qualified manifest resource path if located cleanly; otherwise, null.</returns>
        public static string FindEmbeddedAsset(this Assembly assembly, string primaryKey, string fileExtension = ".wav", params string[] alternativeTokens)
        {
            if (assembly == null || string.IsNullOrWhiteSpace(primaryKey))
                return null;

            // Fetch manifest listings directly from the target reflection space
            string[] resourceNames = assembly.GetManifestResourceNames();

            // Construct structural variants to support alternative dot-to-underscore notation patterns safely
            string sanitizedUnderscoreKey = primaryKey.Replace(".", "_");

            return resourceNames.FirstOrDefault(resource =>
                resource.EndsWith($"{primaryKey}{fileExtension}", StringComparison.OrdinalIgnoreCase) ||
                resource.EndsWith($"{sanitizedUnderscoreKey}{fileExtension}", StringComparison.OrdinalIgnoreCase) ||
                alternativeTokens.Any(token => !string.IsNullOrWhiteSpace(token) && resource.EndsWith($"{token}{fileExtension}", StringComparison.OrdinalIgnoreCase)));
        }
    }
}