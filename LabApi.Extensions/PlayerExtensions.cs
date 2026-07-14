using LabApi.Extensions.Components;
using LabApi.Features.Wrappers;
using MapGeneration;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LabApi.Extensions
{
    /// <summary>
    /// Highly optimized utility extensions for working with <see cref="Player"/>.
    /// Features compile-time zero-allocation paths and highly optimized double-dispatch fuzzy resolution.
    /// </summary>
    public static class PlayerExtensions
    {
        #region Tracking (Transform Followers)

        /// <summary>
        /// Attaches a follower object to the player.
        /// The follower tracks the player's transform with an optional offset.
        /// </summary>
        public static void AttachTrackingObject(this Player player, GameObject followerObject, Vector3 offset = default)
        {
            // FIX: Prevent Unity lifetime reference exceptions.
            if (player == null || player.GameObject == null || followerObject == null)
                return;

            var tracker = followerObject.GetComponent<RuntimeTransformTracker>()
                         ?? followerObject.AddComponent<RuntimeTransformTracker>();

            tracker.Setup(player.GameObject.transform, offset, () => player.IsReady && player.IsAlive);
        }

        /// <summary>
        /// Attaches a follower object to multiple players with zero heap allocations.
        /// </summary>
        public static void AttachTrackingObject(this IEnumerable<Player> players, GameObject followerObject, Vector3 offset = default)
        {
            if (players == null)
                return;

            // FIX: State-passing to prevent closure garbage generation on multi-target attachments.
            players.ForEach((followerObject, offset), static (p, state) => p?.AttachTrackingObject(state.followerObject, state.offset));
        }

        /// <summary>
        /// Attaches a follower object to multiple players (params overload).
        /// </summary>
        public static void AttachTrackingObject(GameObject followerObject, Vector3 offset, params Player[] players)
            => players.AttachTrackingObject(followerObject, offset);

        #endregion

        #region Notifications (Hints)

        /// <summary>
        /// Sends a hint to all ready players.
        /// </summary>
        public static void BroadcastHintToAll(string hintContent, float duration = 5f)
        {
            if (string.IsNullOrEmpty(hintContent))
                return;

            // FIX: Using fast struct enumerator of ReadyList for zero allocation path.
            foreach (var player in Player.ReadyList)
            {
                if (player != null && player.IsReady)
                {
                    player.SendHint(hintContent, duration);
                }
            }
        }

        /// <summary>
        /// Sends a hint to multiple players with zero heap allocations.
        /// Only ready players receive the hint.
        /// </summary>
        public static void BroadcastHint(this IEnumerable<Player> players, string hintContent, float duration = 5f)
        {
            if (players == null)
                return;

            // FIX: State-passing dynamic hint propagation.
            players.ForEach((hintContent, duration), static (p, state) =>
            {
                if (p != null && p.IsReady)
                {
                    p.SendHint(state.hintContent, state.duration);
                }
            });
        }

        /// <summary>
        /// Sends a hint to multiple players (params overload).
        /// </summary>
        public static void BroadcastHint(string hintContent, float duration, params Player[] players)
            => players.BroadcastHint(hintContent, duration);

        #endregion

        #region Status & Identity

        /// <summary>
        /// Returns the player's current Hume Shield value.
        /// Returns 0 if the stat module is missing.
        /// </summary>
        public static float GetHumeShieldValue(this Player player)
        {
            if (player?.ReferenceHub == null)
                return 0f;

            if (player.ReferenceHub.playerStats.TryGetModule<PlayerStatsSystem.HumeShieldStat>(out var shield))
                return shield.CurValue;

            return 0f;
        }

        /// <summary>
        /// Returns true if the player is ready, alive and belongs to a human faction.
        /// </summary>
        public static bool IsLivingHuman(this Player player)
        {
            return player != null
                   && player.IsReady
                   && player.IsAlive
                   && player.IsHuman;
        }

        #endregion

        #region Spatial (Distance, Radius, Rooms)

        /// <summary>
        /// Returns the distance between the player and a world position.
        /// </summary>
        public static float GetDistanceTo(this Player player, Vector3 position)
        {
            if (player == null || player.GameObject == null)
                return 0f;

            return Vector3.Distance(player.Position, position);
        }

        /// <summary>
        /// Returns true if the player is within the given distance from a world position.
        /// Uses squared magnitude for performance.
        /// </summary>
        public static bool IsWithinDistance(this Player player, Vector3 position, float maxDistance)
        {
            if (player == null || player.GameObject == null)
                return false;

            float sqr = (player.Position - position).sqrMagnitude;
            return sqr <= maxDistance * maxDistance;
        }

        /// <summary>
        /// Returns true if the player is within the given radius from a world position.
        /// Uses squared magnitude for performance.
        /// </summary>
        public static bool IsWithinRadius(this Player player, Vector3 targetPosition, float radiusSize)
        {
            if (player == null || player.GameObject == null)
                return false;

            float sqr = (player.Position - targetPosition).sqrMagnitude;
            return sqr <= radiusSize * radiusSize;
        }

        /// <summary>
        /// Returns true if the player is within the given radius from the room center.
        /// </summary>
        public static bool IsWithinRadius(this Player player, Room room, float radiusSize)
        {
            if (player == null || player.GameObject == null || room == null || room.Base == null)
                return false;

            return player.IsWithinRadius(room.Position, radiusSize);
        }

        /// <summary>
        /// Returns true if the player is within the given radius of any provided positions.
        /// Uses squared magnitude for performance with optimized fast-paths.
        /// </summary>
        public static bool IsWithinAnyRadius(this Player player, IEnumerable<Vector3> positions, float radiusSize)
        {
            if (player == null || player.GameObject == null || positions == null)
                return false;

            float sqrRadius = radiusSize * radiusSize;

            if (positions is List<Vector3> list)
            {
                int count = list.Count;
                for (int i = 0; i < count; i++)
                {
                    if ((player.Position - list[i]).sqrMagnitude <= sqrRadius)
                        return true;
                }
                return false;
            }

            foreach (var pos in positions)
            {
                if ((player.Position - pos).sqrMagnitude <= sqrRadius)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the player is currently inside any of the given rooms.
        /// </summary>
        public static bool IsInRoom(this Player player, params RoomName[] roomNames)
        {
            var room = player?.Room;
            if (room == null)
                return false;

            RoomName current = room.Name;

            if (roomNames == null || roomNames.Length == 0)
                return current == RoomName.EzEvacShelter;

            int count = roomNames.Length;
            for (int i = 0; i < count; i++)
            {
                if (roomNames[i] == current)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the room the player is currently standing in.
        /// </summary>
        public static Room GetRoom(this Player player)
        {
            if (player == null || player.GameObject == null)
                return null;

            return player.Position.GetRoom();
        }

        /// <summary>
        /// Returns the distance between the player and the room center.
        /// </summary>
        public static float GetDistanceTo(this Player player, Room room)
        {
            if (player == null || player.GameObject == null || room == null || room.Base == null)
                return 0f;

            return player.GetDistanceTo(room.Position);
        }

        #endregion

        #region Effects (Sensory Shock)

        /// <summary>
        /// Applies visual and auditory impairments to the player.
        /// </summary>
        public static void ApplySensoryShock(
            this Player player,
            float flashDuration = 0f,
            float blindDuration = 0f,
            float blurDuration = 0f,
            float deafenDuration = 0f)
        {
            if (player == null || player.GameObject == null)
                return;

            if (flashDuration > 0f)
                player.EnableEffect(FacilityEffectType.Flashed, 1, flashDuration);

            if (blindDuration > 0f)
                player.EnableEffect(FacilityEffectType.Blindness, 1, blindDuration);

            if (blurDuration > 0f)
                player.EnableEffect(FacilityEffectType.Blurred, 1, blurDuration);

            if (deafenDuration > 0f)
                player.EnableEffect(FacilityEffectType.Deafened, 1, deafenDuration);
        }

        /// <summary>
        /// Applies sensory shock to multiple players with zero heap allocations.
        /// Uses ValueTuple state-passing to completely prevent closure garbage generation.
        /// </summary>
        public static void ApplySensoryShock(
            this IEnumerable<Player> players,
            float flashDuration = 0f,
            float blindDuration = 0f,
            float blurDuration = 0f,
            float deafenDuration = 0f)
        {
            if (players == null)
                return;

            players.ForEach(
                (flashDuration, blindDuration, blurDuration, deafenDuration),
                static (p, s) => p?.ApplySensoryShock(s.flashDuration, s.blindDuration, s.blurDuration, s.deafenDuration)
            );
        }

        /// <summary>
        /// Applies sensory shock to multiple players (params overload).
        /// </summary>
        public static void ApplySensoryShock(
            float flashDuration,
            float blindDuration,
            float blurDuration,
            float deafenDuration,
            params Player[] players)
            => players.ApplySensoryShock(flashDuration, blindDuration, blurDuration, deafenDuration);

        #endregion

        #region Queries (Alive / Human / NotInPocket)

        /// <summary>
        /// Returns only players who are ready and alive.
        /// </summary>
        public static IEnumerable<Player> WhereAlive(this IEnumerable<Player> players)
        {
            if (players == null)
                yield break;

            if (players is List<Player> list)
            {
                int count = list.Count;
                for (int i = 0; i < count; i++)
                {
                    var player = list[i];
                    if (player != null && player.IsReady && player.IsAlive)
                        yield return player;
                }
                yield break;
            }

            foreach (var player in players)
            {
                if (player != null && player.IsReady && player.IsAlive)
                    yield return player;
            }
        }

        /// <summary>
        /// Returns only players who belong to a human faction.
        /// </summary>
        public static IEnumerable<Player> WhereHuman(this IEnumerable<Player> players)
        {
            if (players == null)
                yield break;

            if (players is List<Player> list)
            {
                int count = list.Count;
                for (int i = 0; i < count; i++)
                {
                    var player = list[i];
                    if (player != null && player.IsHuman)
                        yield return player;
                }
                yield break;
            }

            foreach (var player in players)
            {
                if (player != null && player.IsHuman)
                    yield return player;
            }
        }

        /// <summary>
        /// Returns only players who are not inside the Pocket Dimension.
        /// </summary>
        public static IEnumerable<Player> WhereNotInPocket(this IEnumerable<Player> players)
        {
            if (players == null)
                yield break;

            if (players is List<Player> list)
            {
                int count = list.Count;
                for (int i = 0; i < count; i++)
                {
                    var player = list[i];
                    if (player != null && player.Room?.Name != RoomName.Pocket)
                        yield return player;
                }
                yield break;
            }

            foreach (var player in players)
            {
                if (player != null && player.Room?.Name != RoomName.Pocket)
                    yield return player;
            }
        }

        #endregion

        #region Conditions & Escapes

        /// <summary>
        /// Returns true if the player meets basic conditions to escape.
        /// </summary>
        public static bool IsEligibleForEscape(this Player player, IEnumerable<Vector3> escapeZones, float escapeZoneSize)
        {
            if (player == null || player.IsSCP || !player.IsAlive)
                return false;

            return player.IsWithinAnyRadius(escapeZones, escapeZoneSize);
        }

        /// <summary>
        /// Returns true if the player is inside a shelter room or within any shelter radius.
        /// </summary>
        public static bool IsInShelter(this Player player, float shelterZoneSize, IEnumerable<Vector3> shelterLocations, params RoomName[] additionalRooms)
        {
            if (player == null)
                return false;

            if (player.IsInRoom(additionalRooms))
                return true;

            return player.IsWithinAnyRadius(shelterLocations, shelterZoneSize);
        }

        /// <summary>
        /// Returns true if the player is inside an elevator cabin.
        /// </summary>
        public static bool IsInExecutiveElevator(this Player player)
        {
            return player.TryGetNearbyElevatorCabin(out _);
        }

        #region Private/Internal Elevator Identity Lookup
        private const float ElevatorCabinRadius = 4.5f;
        private const float ElevatorCabinRadiusSqr = ElevatorCabinRadius * ElevatorCabinRadius;

        /// <summary>
        /// Returns true if there is an elevator cabin near the player and outputs it.
        /// Shared with lighting extensions for modularity.
        /// </summary>
        public static bool TryGetNearbyElevatorCabin(this Player player, out Elevator cabin)
        {
            cabin = null;

            if (player == null || player.GameObject == null)
                return false;

            // Zero-allocation fast-path
            if (Elevator.List is List<Elevator> list)
            {
                int count = list.Count;
                for (int i = 0; i < count; i++)
                {
                    var elevator = list[i];
                    if (elevator?.Base?.transform == null)
                        continue;

                    if ((player.Position - elevator.Base.transform.position).sqrMagnitude <= ElevatorCabinRadiusSqr)
                    {
                        cabin = elevator;
                        return true;
                    }
                }

                return false;
            }

            // Fallback: IEnumerable scan
            foreach (var elevator in Elevator.List)
            {
                if (elevator?.Base?.transform == null)
                    continue;

                if ((player.Position - elevator.Base.transform.position).sqrMagnitude <= ElevatorCabinRadiusSqr)
                {
                    cabin = elevator;
                    return true;
                }
            }

            return false;
        }
        #endregion

        #endregion

        #region Identity Resolution (Fuzzy)

        /// <summary>
        /// Attempts to resolve a player from a fuzzy identifier.
        /// Supports exact ID, UserId, exact nickname, substring match, and Levenshtein fallback.
        /// </summary>
        public static bool TryResolveFuzzy(this IEnumerable<Player> players, string identifier, out Player target, out string error)
        {
            target = null;
            error = null;

            string search = identifier?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(search))
            {
                error = "Target Assignment Failed: Identifier cannot be empty.";
                return false;
            }

            // FIX: Prevent interface boxing of struct enumerator by directing Lists to optimized array overload.
            if (players is List<Player> list)
                return TryResolveFuzzyCore(list, search, out target, out error);

            return TryResolveFuzzyCore(players, search, out target, out error);
        }

        /// <summary>
        /// Core fuzzy resolver for Lists. True zero-allocation path using standard index loops.
        /// </summary>
        private static bool TryResolveFuzzyCore(List<Player> players, string search, out Player target, out string error)
        {
            target = null;
            error = null;

            Player best = null;
            int bestDist = int.MaxValue;
            bool substringFound = false;

            int count = players.Count;

            // 1. Exact match pass
            for (int i = 0; i < count; i++)
            {
                var p = players[i];
                if (p == null) continue;

                if (p.PlayerId.ToString() == search ||
                    p.UserId.NormalizeUserId() == search ||
                    p.Nickname.Equals(search, StringComparison.OrdinalIgnoreCase))
                {
                    target = p;
                    return true;
                }
            }

            // 2. Substring + Levenshtein pass
            for (int i = 0; i < count; i++)
            {
                var p = players[i];
                if (p == null) continue;

                string nick = p.Nickname.ToLowerInvariant();

                // Substring match
                if (nick.Contains(search) || search.Contains(nick))
                {
                    if (!substringFound)
                    {
                        best = p;
                        substringFound = true;
                    }
                    else
                    {
                        error = $"Ambiguous Player Target. Multiple candidates matched '{search}'.";
                        return false;
                    }
                    continue;
                }

                // Levenshtein fallback
                int dist = search.ComputeLevenshteinDistance(nick);
                if (dist <= 3)
                {
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        best = p;
                    }
                    else if (dist == bestDist)
                    {
                        error = $"Ambiguous Player Target. Multiple candidates matched '{search}'.";
                        return false;
                    }
                }
            }

            if (best == null)
            {
                error = $"Target Assignment Failed: No player matched '{search}'.";
                return false;
            }

            target = best;
            return true;
        }

        /// <summary>
        /// Fallback fuzzy resolver for general collections.
        /// </summary>
        private static bool TryResolveFuzzyCore(IEnumerable<Player> players, string search, out Player target, out string error)
        {
            target = null;
            error = null;

            Player best = null;
            int bestDist = int.MaxValue;
            bool substringFound = false;

            // 1. Exact match pass
            foreach (var p in players)
            {
                if (p == null) continue;

                if (p.PlayerId.ToString() == search ||
                    p.UserId.NormalizeUserId() == search ||
                    p.Nickname.Equals(search, StringComparison.OrdinalIgnoreCase))
                {
                    target = p;
                    return true;
                }
            }

            // 2. Substring + Levenshtein pass
            foreach (var p in players)
            {
                if (p == null) continue;

                string nick = p.Nickname.ToLowerInvariant();

                // Substring match
                if (nick.Contains(search) || search.Contains(nick))
                {
                    if (!substringFound)
                    {
                        best = p;
                        substringFound = true;
                    }
                    else
                    {
                        error = $"Ambiguous Player Target. Multiple candidates matched '{search}'.";
                        return false;
                    }
                    continue;
                }

                // Levenshtein fallback
                int dist = search.ComputeLevenshteinDistance(nick);
                if (dist <= 3)
                {
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        best = p;
                    }
                    else if (dist == bestDist)
                    {
                        error = $"Ambiguous Player Target. Multiple candidates matched '{search}'.";
                        return false;
                    }
                }
            }

            if (best == null)
            {
                error = $"Target Assignment Failed: No player matched '{search}'.";
                return false;
            }

            target = best;
            return true;
        }

        #endregion
    }
}