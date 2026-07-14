using LabApi.Extensions.Misc;
using System;
using System.IO;

namespace LabApi.Extensions
{
    /// <summary>
    /// Highly optimized utility extensions for filesystem checks and file operations.
    /// Employs lazy enumeration and state-passing loops to guarantee minimal heap allocations.
    /// </summary>
    public static class IoExtensions
    {
        /// <summary>
        /// Returns true if the path exists and is a reparse point (symlink, junction, etc.).
        /// Bypasses redundant filesystem existance checks to minimize disk IO roundtrips.
        /// </summary>
        public static bool IsReparsePoint(this string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            try
            {
                // FIX: Removed Directory.Exists and File.Exists pre-checks.
                // File.GetAttributes works for both files/directories and naturally fails if path doesn't exist.
                // This reduces disk metadata lookups from 3 calls down to 1.
                return (File.GetAttributes(path) & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
            }
            catch
            {
                // Ignore IO exceptions safely (missing file, locked file, permission issues)
                return false;
            }
        }

        /// <summary>
        /// Copies files from one directory to another using a search pattern.
        /// Features lazy file enumeration and state-passing loops to prevent massive string array allocations.
        /// </summary>
        public static void CopyFilesTo(
            this string sourceDirectory,
            string destinationDirectory,
            string searchPattern = "*.*",
            bool overwrite = true)
        {
            if (string.IsNullOrEmpty(sourceDirectory) || string.IsNullOrEmpty(destinationDirectory))
                return;

            if (!Directory.Exists(sourceDirectory))
                return;

            if (!Directory.Exists(destinationDirectory))
                Directory.CreateDirectory(destinationDirectory);

            try
            {
                // FIX: Used lazy-evaluated Directory.EnumerateFiles to completely bypass massive string[] array allocations.
                // Spun up our state-passing ForEach to safely pass the copy parameters over the stack.
                Directory.EnumerateFiles(sourceDirectory, searchPattern).ForEach(
                    (destinationDirectory, overwrite),
                    static (src, state) =>
                    {
                        string name = Path.GetFileName(src);
                        string dst = Path.Combine(state.destinationDirectory, name);

                        if (state.overwrite || !File.Exists(dst))
                        {
                            File.Copy(src, dst, state.overwrite);
                        }
                    });
            }
            catch (Exception ex)
            {
                iLogger.Error("IoExtensions.CopyFilesTo",
                    $"File copy failed for pattern '{searchPattern}': {ex.Message}");
            }
        }
    }
}