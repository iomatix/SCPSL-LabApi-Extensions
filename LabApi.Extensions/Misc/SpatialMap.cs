using LabApi.Features.Wrappers;
using PlayerRoles.Ragdolls;
using System;
using UnityEngine;

namespace LabApi.Extensions.Misc
{
    /// <summary>
    /// Legacy spatial lookup and entity resolution queries. Kept as a compatibility bridge.
    /// </summary>
    [Obsolete("SpatialMap is deprecated and will be removed in version 1.0.0. Please use modern extension methods: Vector3.GetRoom(), ReferenceHub.GetPlayer(), and BasicRagdoll.GetRagdoll() directly.", false)]
    public static class SpatialMap
    {
        /// <summary>
        /// Retrieves a randomly selected <see cref="Elevator"/> instance.
        /// </summary>
        [Obsolete("Use Map.GetRandomElevator() directly instead.", false)]
        public static Elevator GetRandomElevator()
            => Map.GetRandomElevator();

        /// <summary>
        /// Resolves a 3D coordinate vector to its corresponding <see cref="Room"/> containment bounds.
        /// </summary>
        [Obsolete("Use position.GetRoom() extension from VectorExtensions instead.", false)]
        public static Room GetRoomAtPosition(Vector3 position)
            => position.GetRoom();

        /// <summary>
        /// Resolves a wrapped <see cref="Player"/> instance from a native <see cref="ReferenceHub"/>.
        /// </summary>
        [Obsolete("Use referenceHub.GetPlayer() extension from PlayerExtensions instead.", false)]
        public static Player GetPlayer(ReferenceHub referenceHub)
            => referenceHub.GetPlayer();

        /// <summary>
        /// Resolves a wrapped <see cref="Ragdoll"/> instance from a native <see cref="BasicRagdoll"/>.
        /// </summary>
        [Obsolete("Use ragdoll.GetRagdoll() extension from RagdollExtensions instead.", false)]
        public static Ragdoll GetRagdoll(BasicRagdoll ragdoll)
            => ragdoll.GetRagdoll();
    }
}