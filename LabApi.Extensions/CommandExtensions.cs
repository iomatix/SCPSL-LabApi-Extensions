using CommandSystem;
using RemoteAdmin;
using System;

namespace LabApi.Extensions
{
    /// <summary>
    /// Provides high-performance utility extensions for CommandSystem routing, permission gates, and array segment parsing layouts.
    /// </summary>
    internal static class CommandExtensions
    {
        /// <summary>
        /// Defensively verifies if the command sender possesses the required administrative permission tracking profile.
        /// Automatically grants authorization if the command originated from the native Server Console.
        /// </summary>
        /// <param name="sender">The active command sender invocation context layer to validate.</param>
        /// <param name="permission">The required clearance baseline to check against.</param>
        /// <param name="errorResponse">The standardized fallback response message emitted natively if the permission check fails.</param>
        /// <returns><c>true</c> if authorization is successfully cleared or bypassed; otherwise, <c>false</c>.</returns>
        public static bool ConfirmPermission(this ICommandSender sender, PlayerPermissions permission, out string errorResponse)
        {
            if (sender is PlayerCommandSender playerSender)
            {
                if (!playerSender.CheckPermission(permission))
                {
                    errorResponse = $"Unauthorized Command Execution: You do not possess the required administrative clearance ({permission}) to trigger this channel.";
                    return false;
                }
            }

            errorResponse = null;
            return true;
        }

        /// <summary>
        /// Safely attempts to extract and parse a single-precision floating-point scalar variable from the raw arguments segment layout.
        /// </summary>
        /// <param name="arguments">The raw token array context passed by the engine command solver.</param>
        /// <param name="index">The targeting array zero-based index slot allocation to parse.</param>
        /// <param name="value">The successfully extracted output float scalar variable, or <c>0.0f</c> if validation fails.</param>
        /// <param name="minValue">The optional minimum structural constraint required to pass validation boundaries.</param>
        /// <returns><c>true</c> if the token exists, conforms cleanly to numerical constraints, and satisfies boundary limits; otherwise, <c>false</c>.</returns>
        public static bool TryGetFloat(this ArraySegment<string> arguments, int index, out float value, float minValue = float.MinValue)
        {
            value = 0f;
            if (arguments.Count <= index) return false;

            // Use memory-safe direct bounds extraction matching framework memory layout boundaries without allocations
            string rawToken = arguments.Array?[arguments.Offset + index];
            if (string.IsNullOrEmpty(rawToken)) return false;

            if (float.TryParse(rawToken, out float parsed) && parsed >= minValue)
            {
                value = parsed;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Safely attempts to extract and parse a signed 32-bit integer data variable from the raw arguments segment layout.
        /// </summary>
        /// <param name="arguments">The raw token array context passed by the engine command solver.</param>
        /// <param name="index">The targeting array zero-based index slot allocation to parse.</param>
        /// <param name="value">The successfully extracted output integer variable, or <c>0</c> if validation fails.</param>
        /// <param name="minValue">The optional minimum structural constraint required to pass validation boundaries.</param>
        /// <returns><c>true</c> if the token exists, conforms cleanly to numerical constraints, and satisfies boundary limits; otherwise, <c>false</c>.</returns>
        public static bool TryGetInt(this ArraySegment<string> arguments, int index, out int value, int minValue = int.MinValue)
        {
            value = 0;
            if (arguments.Count <= index) return false;

            string rawToken = arguments.Array?[arguments.Offset + index];
            if (string.IsNullOrEmpty(rawToken)) return false;

            if (int.TryParse(rawToken, out int parsed) && parsed >= minValue)
            {
                value = parsed;
                return true;
            }

            return false;
        }
    }
}