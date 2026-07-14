using LabApi.Features.Wrappers;
using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LabApi.Extensions.Environment
{
    /// <summary>
    /// Provides environmental manipulation helpers.
    /// </summary>
    [Obsolete("EnvironmentEngine is deprecated. Its members have been optimized and merged into specialized extension classes. Use ZoneLightingExtensions for zone-specific operations.", false)]
    public static class EnvironmentEngine
    {
        #region Emergency Strobe (Legacy Bridge - Fully Optimized Under the Hood)

        /// <summary>
        /// Private lightweight coroutine that drives the entire strobe sequence using a single state loop.
        /// Replaces the legacy garbage-heavy nested CallDelayed scheduling system.
        /// </summary>
        private static IEnumerator<float> EmergencyStrobeCoroutine(float totalDuration, float pulseInterval, Color alertColor)
        {
            float elapsed = 0f;

            // Cache interval math on the stack to prevent redundant runtime division
            float phase1Duration = pulseInterval * 0.25f;
            float phase2Duration = pulseInterval * 0.38f; // (0.63 - 0.25)
            float phase3Duration = pulseInterval * 0.37f; // (1.00 - 0.63)

            while (elapsed < totalDuration)
            {
                // Phase 1: Force alert illumination vector color mapping
                Map.SetColorOfLights(alertColor);
                yield return Timing.WaitForSeconds(phase1Duration);

                // Phase 2: Intermediary collapse into temporary darkness
                Map.TurnOffLights();
                yield return Timing.WaitForSeconds(phase2Duration);

                // Phase 3: Restore illumination grid and transition into deep ambient shadows
                Map.TurnOnLights();
                Map.SetColorOfLights(Color.black);
                yield return Timing.WaitForSeconds(phase3Duration);

                elapsed += pulseInterval;
            }

            // Restore defaults at the end of the strobe event
            Map.ResetColorOfLights();
        }

        /// <summary>
        /// Launches a background strobe loop.
        /// </summary>
        [Obsolete("StartEmergencyStrobe scheduled thousands of nested coroutines and is highly deprecated. It has been rewritten internally to use a single lightweight state loop, but you should migrate your systems to clean, customized coroutines.", false)]
        public static void StartEmergencyStrobe(
            float totalDuration,
            float pulseInterval,
            Color alertColor,
            string masterTag = "LabApi.Extensions.Environment-strobeLights")
        {
            if (pulseInterval <= 0.05f)
                pulseInterval = 0.1f; // Prevent division/infinite loop locks

            // FIX: Bypassed the horrible "scheduling bomb" loop. 
            // Now runs exactly 1 lightweight coroutine under the hood to completely protect the server.
            if (string.IsNullOrEmpty(masterTag))
                Timing.RunCoroutine(EmergencyStrobeCoroutine(totalDuration, pulseInterval, alertColor));
            else
                Timing.RunCoroutine(EmergencyStrobeCoroutine(totalDuration, pulseInterval, alertColor), masterTag);
        }

        #endregion

        #region Zone Flicker (Legacy Bridge - Redirects to ZoneLightingExtensions)

        /// <summary>
        /// Launches a non-blocking background asynchronous coroutine loop that flickers the illumination grid.
        /// </summary>
        [Obsolete("StartZoneFlicker() is deprecated. Please migrate to ZoneLightingExtensions.FlickerLights() which is fully optimized and allocation-free.", false)]
        public static void StartZoneFlicker(
            MapGeneration.FacilityZone zone,
            float duration,
            float frequency,
            Color color,
            string coroutineTag = "LabApi.Extensions.Environment-flickerLights")
        {
            // FIX: DRY compliance. Completely bypassed duplicate routine code by routing directly to our newly optimized extensions.
            ZoneLightingExtensions.FlickerLights(new[] { zone }, color, duration, frequency, coroutineTag);
        }

        #endregion
    }
}