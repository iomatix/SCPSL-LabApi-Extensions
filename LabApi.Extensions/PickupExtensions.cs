using LabApi.Extensions.Misc;
using LabApi.Features.Wrappers;
using System.Collections.Generic;
using UnityEngine;

namespace LabApi.Extensions
{
    /// <summary>
    /// Provides high-performance utility abstraction layers for managing dynamic items and active world-space pickup assets.
    /// </summary>
    public static class PickupExtensions
    {
        /// <summary>
        /// Forcibly applies kinetic physical propulsion forces onto a single world-space pickup asset.
        /// Single Point of Truth for individual pickup physics manipulation.
        /// </summary>
        /// <param name="pickup">The target pickup entity undergoing physical acceleration.</param>
        /// <param name="linearVelocityMagnitude">The directional scale baseline applied to drive linear movement trajectories.</param>
        /// <param name="angularVelocityMagnitude">The rotational torque scale baseline applied to trigger spinning effects.</param>
        public static void ApplyKineticBlast(this Pickup pickup, float linearVelocityMagnitude, float angularVelocityMagnitude)
        {
            if (pickup is null || pickup.IsDestroyed || !pickup.IsSpawned) return;

            Rigidbody rb = pickup.Rigidbody;
            if (rb is null) return;

            try
            {
                rb.isKinematic = false;

                // Leverage our math extensions directly to compute floor deflection patterns
                Vector3 forceDirection = VectorExtensions.GetRandomUpwardSphereVelocity(linearVelocityMagnitude);

                rb.linearVelocity = forceDirection;
                rb.angularVelocity = Random.insideUnitSphere * angularVelocityMagnitude;
            }
            catch (System.Exception ex)
            {
                iLogger.Error("PickupExtensions.KineticBlast", $"Failed to apply physical velocity to pickup item: {ex.Message}");
            }
        }

        #region Batch Operations (Zero-Allocation High-Performance Overloads)

        /// <summary>
        /// Iterates over an aggregated collection layout of spawned pickups and forcibly applies batch kinetic physical propulsion forces.
        /// Seamlessly delegates item execution to maintain absolute single responsibility and zero duplication.
        /// </summary>
        /// <param name="pickups">The target enumerable stream tracking world item entities undergoing physical acceleration.</param>
        /// <param name="linearVelocityMagnitude">The directional scale baseline applied to drive linear movement trajectories.</param>
        /// <param name="angularVelocityMagnitude">The rotational torque scale baseline applied to trigger spinning effects.</param>
        public static void ApplyKineticBlast(this IEnumerable<Pickup> pickups, float linearVelocityMagnitude, float angularVelocityMagnitude)
        {
            if (pickups is null) return;

            // Zero-allocation batch processing optimization
            if (pickups is List<Pickup> concreteList)
            {
                int count = concreteList.Count;
                for (int i = 0; i < count; i++)
                {
                    concreteList[i].ApplyKineticBlast(linearVelocityMagnitude, angularVelocityMagnitude);
                }
                return;
            }

            foreach (Pickup pickup in pickups)
            {
                pickup.ApplyKineticBlast(linearVelocityMagnitude, angularVelocityMagnitude);
            }
        }

        /// <summary>
        /// Iterates over an inline array of spawned pickups and forcibly applies batch kinetic physical propulsion forces.
        /// </summary>
        /// <param name="linearVelocityMagnitude">The directional scale baseline applied to drive linear movement trajectories.</param>
        /// <param name="angularVelocityMagnitude">The rotational torque scale baseline applied to trigger spinning effects.</param>
        /// <param name="pickups">The target inline array tracking world item entities undergoing physical acceleration.</param>
        public static void ApplyKineticBlast(float linearVelocityMagnitude, float angularVelocityMagnitude, params Pickup[] pickups)
        {
            if (pickups is null) return;

            int count = pickups.Length;
            for (int i = 0; i < count; i++)
            {
                pickups[i].ApplyKineticBlast(linearVelocityMagnitude, angularVelocityMagnitude);
            }
        }

        #endregion
    }
}