using LabApi.Extensions.Misc;
using LabApi.Features.Wrappers;
using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LabApi.Extensions
{
    /// <summary>
    /// Provides fail-safe structural illumination manipulation pipelines for moving lift cabin primitives.
    /// </summary>
    internal static class ElevatorLightingExtensions
    {
        private static readonly HashSet<int> _blackedOutElevatorIds = new();
        private static readonly object _syncLock = new();

        /// <summary>
        /// Verified Boolean Query: Determines with zero heap allocations whether an elevator instance is undergoing active power suppression.
        /// </summary>
        public static bool AreLightsOff(this Elevator elevator)
        {
            if (elevator?.Base is null) return false;
            lock (_syncLock)
            {
                return _blackedOutElevatorIds.Contains(elevator.Base.GetInstanceID());
            }
        }

        /// <summary>
        /// Forcibly suppresses all illumination and mesh rendering emission vectors inside the elevator cabin for a specific duration.
        /// </summary>
        /// <param name="elevator">The target elevator instance undergoing cabin blackout.</param>
        /// <param name="duration">The chronological duration window in seconds during which the cabin remains pitch black.</param>
        public static void TurnOffLights(this Elevator elevator, float duration)
        {
            if (elevator?.Base?.gameObject is null) return;
            int instanceId = elevator.Base.GetInstanceID();

            lock (_syncLock)
            {
                _blackedOutElevatorIds.Add(instanceId);
            }

            // Client-side visual sync: Deep-search and disable all rendering components inside the active chamber structure securely
            ToggleCabinVisualEmitters(elevator.Base.gameObject, false);

            // Automated thread-safe power restoration pipeline loop using MEC delay tokens
            Timing.CallDelayed(duration, () => elevator.TurnOnLights());
        }

        /// <summary>
        /// Restores standard electrical power and re-activates all internal mesh illumination parameters inside the elevator cabin.
        /// </summary>
        /// <param name="elevator">The target elevator instance undergoing light grid restoration.</param>
        public static void TurnOnLights(this Elevator elevator)
        {
            if (elevator?.Base?.gameObject is null) return;
            int instanceId = elevator.Base.GetInstanceID();

            lock (_syncLock)
            {
                _blackedOutElevatorIds.Remove(instanceId);
            }

            ToggleCabinVisualEmitters(elevator.Base.gameObject, true);
        }

        /// <summary>
        /// Advanced Failsafe Cascade: Scans and suppresses renderers and standalone lights dynamically without tight framework assembly coupling.
        /// </summary>
        private static void ToggleCabinVisualEmitters(GameObject elevatorRoot, bool state)
        {
            try
            {
                // Action 1: Disabling any legacy point/spot illumination objects discovered inside the hierarchy
                foreach (Light light in elevatorRoot.GetComponentsInChildren<Light>(true))
                {
                    if (light is not null) light.enabled = state;
                }

                // Action 2: Pulling MeshRenderers dynamically to instantly terminate HDRP Tube/Disc light shaders emission layers
                foreach (MeshRenderer renderer in elevatorRoot.GetComponentsInChildren<MeshRenderer>(true))
                {
                    if (renderer is null) continue;

                    string nameLower = renderer.name.ToLowerInvariant();
                    if (nameLower.Contains("light") || nameLower.Contains("lamp") || nameLower.Contains("glow") || nameLower.Contains("emit"))
                    {
                        renderer.enabled = state;
                    }
                }

                // Action 3: Target native script components handling visual fluctuations defensively via structural names
                foreach (MonoBehaviour behavior in elevatorRoot.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (behavior is null) continue;

                    string typeName = behavior.GetType().Name;
                    if (typeName == "FlickerableLight" || typeName == "TubeLight" || typeName == "DiscLight")
                    {
                        behavior.enabled = state;
                    }
                }
            }
            catch (Exception ex)
            {
                iLogger.Error("ElevatorLighting.Cascade", $"Anomalous rendering track state manipulation interruption: {ex.Message}");
            }
        }

        /// <summary>
        /// Fluently executes a synchronized asynchronous lighting flicker animation loop over an individual elevator cabin space.
        /// </summary>
        public static IEnumerator<float> FlickerElevatorLightsCoroutine(this Elevator elevator, float duration, float frequency)
        {
            if (elevator?.Base?.gameObject is null) yield break;

            float interval = 1f / frequency.LimitMin(0.1f);
            int flickers = (int)Math.Round(duration / interval);

            for (int i = 0; i < flickers; i++)
            {
                if (elevator.Base?.gameObject is null) yield break;

                ToggleCabinVisualEmitters(elevator.Base.gameObject, false);
                yield return Timing.WaitForSeconds(interval * 0.5f);

                if (elevator.Base?.gameObject is null) yield break;

                ToggleCabinVisualEmitters(elevator.Base.gameObject, true);
                yield return Timing.WaitForSeconds(interval * 0.5f);
            }
        }
    }
}