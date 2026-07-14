using LabApi.Features.Wrappers;
using PlayerRoles.Ragdolls;

namespace LabApi.Extensions
{
    /// <summary>
    /// Utility extensions for working with Ragdoll entities.
    /// </summary>
    public static class RagdollExtensions
    {
        /// <summary>
        /// Resolves the API-wrapped <see cref="Ragdoll"/> proxy from this native low-level Unity <see cref="BasicRagdoll"/>.
        /// </summary>
        public static Ragdoll GetRagdoll(this BasicRagdoll ragdoll)
        {
            return ragdoll == null ? null : Ragdoll.Get(ragdoll);
        }
    }
}