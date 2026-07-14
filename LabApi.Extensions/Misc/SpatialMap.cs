using LabApi.Features.Wrappers;
using UnityEngine;

namespace LabApi.Extensions.Misc
{
    /// <summary>
    /// Provides global spatial lookup queries, entity resolution, and factory wrappers for active game world objects.
    /// </summary>
    public static class SpatialMap
    {
        /// <summary>
        /// Retrieves a randomly selected <see cref="Elevator"/> instance from the active global facility map.
        /// </summary>
        public static Elevator GetRandomElevator() => Map.GetRandomElevator();

        /// <summary>
        /// Resolves a 3D coordinate vector to its corresponding <see cref="Room"/> containment bounds.
        /// </summary>
        public static Room GetRoomAtPosition(Vector3 position) => Room.GetRoomAtPosition(position);

        /// <summary>
        /// Resolves a wrapped <see cref="Player"/> instance from a native low-level <see cref="ReferenceHub"/>.
        /// </summary>
        public static Player GetPlayer(ReferenceHub referenceHub)
            => referenceHub == null ? null : Player.Get(referenceHub);

        /// <summary>
        /// Resolves a wrapped <see cref="Ragdoll"/> instance from a native low-level <see cref="PlayerRoles.Ragdolls.BasicRagdoll"/>.
        /// </summary>
        public static Ragdoll GetRagdoll(PlayerRoles.Ragdolls.BasicRagdoll ragdoll)
            => ragdoll == null ? null : Ragdoll.Get(ragdoll);
    }
}