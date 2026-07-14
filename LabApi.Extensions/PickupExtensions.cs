using LabApi.Features.Wrappers;
using System.Collections.Generic;
using UnityEngine;

namespace LabApi.Extensions
{
    /// <summary>
    /// Highly optimized utility extensions for applying physics impulses to world pickups.
    /// Employs state-passing loops and strict Unity lifetime checks to guarantee zero heap allocations.
    /// </summary>
    public static class PickupExtensions
    {
        #region Single Pickup

        /// <summary>
        /// Applies a physics impulse to a pickup safely.
        /// </summary>
        public static void ApplyKineticBlast(this Pickup pickup, float linearVelocityMagnitude, float angularVelocityMagnitude)
        {
            // FIX: Safe Unity reference verification instead of 'is null'.
            if (pickup == null || pickup.IsDestroyed || !pickup.IsSpawned)
                return;

            var rb = pickup.Rigidbody;

            // FIX: Prevented Unity lifetime bypass. 'is null' replaced with safe operator '== null'.
            if (rb == null)
                return;

            rb.isKinematic = false;

            Vector3 force = VectorExtensions.GetRandomUpwardSphereVelocity(linearVelocityMagnitude);
            rb.linearVelocity = force;
            rb.angularVelocity = Random.insideUnitSphere * angularVelocityMagnitude;
        }

        #endregion

        #region Batch Operations (Zero-Allocation via State-Passing)

        /// <summary>
        /// Applies a physics impulse to multiple pickups with 0 heap allocations.
        /// </summary>
        public static void ApplyKineticBlast(this IEnumerable<Pickup> pickups, float linearVelocityMagnitude, float angularVelocityMagnitude)
        {
            if (pickups == null)
                return;

            // FIX: Enforced state-passing and static lambda to completely eliminate closure allocation spikes on explosions.
            pickups.ForEach(
                (linearVelocityMagnitude, angularVelocityMagnitude),
                static (p, state) => p?.ApplyKineticBlast(state.linearVelocityMagnitude, state.angularVelocityMagnitude)
            );
        }

        /// <summary>
        /// Applies a physics impulse to multiple pickups (params overload).
        /// </summary>
        public static void ApplyKineticBlast(float linearVelocityMagnitude, float angularVelocityMagnitude, params Pickup[] pickups)
        {
            if (pickups == null)
                return;

            pickups.ApplyKineticBlast(linearVelocityMagnitude, angularVelocityMagnitude);
        }

        #endregion
    }
}