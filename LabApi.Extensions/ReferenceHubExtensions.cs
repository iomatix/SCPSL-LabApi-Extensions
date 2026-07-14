using LabApi.Features.Wrappers;

namespace LabApi.Extensions
{
    /// <summary>
    /// Utility extensions for working with native <see cref="ReferenceHub"/> components.
    /// </summary>
    public static class ReferenceHubExtensions
    {
        /// <summary>
        /// Resolves the strongly-typed LabAPI <see cref="Player"/> wrapper from this native <see cref="ReferenceHub"/> safely.
        /// </summary>
        /// <param name="referenceHub">The native ReferenceHub component instance.</param>
        /// <returns>The API-wrapped <see cref="Player"/> proxy; otherwise, <see langword="null"/> if the hub is invalid or destroyed.</returns>
        public static Player GetPlayer(this ReferenceHub referenceHub)
        {
            // FIX: Using traditional == null for native Unity-derived components to prevent fake null-reference bypasses.
            return referenceHub == null ? null : Player.Get(referenceHub);
        }
    }
}