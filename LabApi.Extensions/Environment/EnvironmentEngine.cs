using System;
using UnityEngine;

namespace LabApi.Extensions.Environment
{
    /// <summary>
    /// Legacy environment manipulation helpers.
    /// </summary>
    [Obsolete("EnvironmentEngine is deprecated. Its members have been optimized and merged into specialized extension classes. Please migrate to MapExtensions for global actions and ZoneLightingExtensions for zone-specific actions.", false)]
    public static class EnvironmentEngine
    {
        /// <summary>
        /// Launches a background strobe loop.
        /// </summary>
        [Obsolete("StartEmergencyStrobe() has been moved and fully optimized. Please migrate to MapExtensions.StartEmergencyStrobe() to prevent compilation warnings.", false)]
        public static void StartEmergencyStrobe(
            float totalDuration,
            float pulseInterval,
            Color alertColor,
            string masterTag = "LabApi.Extensions.Environment-strobeLights")
        {
            // FIX: Business logic preserved! Redirects directly to the new, highly optimized global engine in MapExtensions.
            MapExtensions.StartEmergencyStrobe(totalDuration, pulseInterval, alertColor, masterTag);
        }

        /// <summary>
        /// Launches a background asynchronous coroutine loop that flickers the illumination grid of a specific zone.
        /// </summary>
        [Obsolete("StartZoneFlicker() is deprecated. Please migrate to ZoneLightingExtensions.FlickerLights() which is fully optimized and allocation-free.", false)]
        public static void StartZoneFlicker(
            MapGeneration.FacilityZone zone,
            float duration,
            float frequency,
            Color color,
            string coroutineTag = "LabApi.Extensions.Environment-flickerLights")
        {
            // FIX: Redirects cleanly to the optimized ZoneLightingExtensions.
            ZoneLightingExtensions.FlickerLights(new[] { zone }, color, duration, frequency, coroutineTag);
        }
    }
}