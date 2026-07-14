using LabApi.Extensions.Misc;
using LabApi.Features.Wrappers;
using System;
using UnityEngine;

namespace LabApi.Extensions
{
    /// <summary>
    /// Thread-safe, high-performance utility extensions for Unity Vector3.
    /// Bypasses native engine boundary transitions and UnityEngine.Random to maximize speed and thread compatibility.
    /// </summary>
    public static class VectorExtensions
    {
        #region Distance Calculations (Thread-Safe)

        /// <summary>
        /// Returns the distance between two points.
        /// </summary>
        public static float GetDistanceTo(this Vector3 origin, Vector3 target)
        {
            float dx = origin.x - target.x;
            float dy = origin.y - target.y;
            float dz = origin.z - target.z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>
        /// Returns the squared distance between two points with zero square root overhead.
        /// </summary>
        public static float GetDistanceSquaredTo(this Vector3 origin, Vector3 target)
        {
            // FIX: Bypassed Vector3.SqrMagnitude native transition. Pure math multiplication is highly optimized by JIT.
            float dx = origin.x - target.x;
            float dy = origin.y - target.y;
            float dz = origin.z - target.z;
            return dx * dx + dy * dy + dz * dz;
        }

        /// <summary>
        /// Returns true if the target is within the specified range from the origin.
        /// Highly optimized utilizing squared distances to avoid expensive square root operations.
        /// </summary>
        public static bool IsWithinRange(this Vector3 origin, Vector3 target, float range)
        {
            float dx = origin.x - target.x;
            float dy = origin.y - target.y;
            float dz = origin.z - target.z;
            return (dx * dx + dy * dy + dz * dz) <= range * range;
        }

        #endregion

        #region Flat 2D Distance Checks (Ignoring Y Axis)

        /// <summary>
        /// Returns the distance between two points ignoring the Y axis (great for same-floor evaluations).
        /// </summary>
        public static float FlatDistanceTo(this Vector3 origin, Vector3 target)
        {
            float dx = origin.x - target.x;
            float dz = origin.z - target.z;
            return (float)Math.Sqrt(dx * dx + dz * dz);
        }

        /// <summary>
        /// Returns the squared distance between two points ignoring the Y axis with zero square root overhead.
        /// </summary>
        public static float FlatDistanceSquaredTo(this Vector3 origin, Vector3 target)
        {
            float dx = origin.x - target.x;
            float dz = origin.z - target.z;
            return dx * dx + dz * dz;
        }

        #endregion

        #region Modifications & Direction helpers

        /// <summary>
        /// Safely returns a normalized direction vector pointing from origin to target.
        /// Features built-in zero-division safety.
        /// </summary>
        public static Vector3 DirectionTo(this Vector3 origin, Vector3 target)
        {
            float dx = target.x - origin.x;
            float dy = target.y - origin.y;
            float dz = target.z - origin.z;
            float sqrMag = dx * dx + dy * dy + dz * dz;

            if (sqrMag < 1e-10f)
                return Vector3.zero;

            float invMag = 1f / (float)Math.Sqrt(sqrMag);
            return new Vector3(dx * invMag, dy * invMag, dz * invMag);
        }

        /// <summary>
        /// Returns a new Vector3 with a modified X coordinate.
        /// </summary>
        public static Vector3 WithX(this Vector3 vector, float x) => new Vector3(x, vector.y, vector.z);

        /// <summary>
        /// Returns a new Vector3 with a modified Y coordinate.
        /// </summary>
        public static Vector3 WithY(this Vector3 vector, float y) => new Vector3(vector.x, y, vector.z);

        /// <summary>
        /// Returns a new Vector3 with a modified Z coordinate.
        /// </summary>
        public static Vector3 WithZ(this Vector3 vector, float z) => new Vector3(vector.x, vector.y, z);

        #endregion

        #region Vector Projection & Randomization

        /// <summary>
        /// Reflects the vector upward if it points too far downward.
        /// Optimally negated in-place without invoking heavy engine reflection methods.
        /// </summary>
        public static Vector3 GetUpwardReflectedVector(this Vector3 direction, float dotThreshold = 0.707f)
        {
            // FIX: Since down is (0, -1, 0), the dot product is simply -y.
            // Reflecting over up (0, 1, 0) simply negates the y coordinate.
            // This achieves 0 native engine calls and executes instantly.
            return -direction.y > dotThreshold
                ? new Vector3(direction.x, -direction.y, direction.z)
                : direction;
        }

        /// <summary>
        /// Returns a random upward-facing direction scaled by the given magnitude.
        /// Thread-safe and powered by SafeRandom.
        /// </summary>
        public static Vector3 GetRandomUpwardSphereVelocity(float magnitude = 1f)
        {
            // FIX: Bypassed non-thread-safe UnityEngine.Random. 
            // Generates point uniformly distributed on a unit sphere using exact trigonometry.
            float z = SafeRandom.Range(-1f, 1f);
            float phi = SafeRandom.Range(0f, 2f * (float)Math.PI);
            float r = (float)Math.Sqrt(1f - z * z);

            float x = r * (float)Math.Cos(phi);
            float y = r * (float)Math.Sin(phi);

            Vector3 randomDirection = new Vector3(x, y, z);
            return randomDirection.GetUpwardReflectedVector(0.707f) * magnitude;
        }

        #endregion

        #region 
        /// <summary>
        /// Resolves and returns the room at this specific 3D spatial coordinate vector.
        /// </summary>
        public static Room GetRoom(this Vector3 position) =>
            Room.GetRoomAtPosition(position);

        #endregion

        #region Sanitization

        /// <summary>
        /// Returns the vector unless it contains NaN or Infinity, in which case the fallback is returned.
        /// </summary>
        public static Vector3 Sanitize(this Vector3 vector, Vector3 fallback = default)
        {
            return vector.x.IsNanOrInfinity() ||
                   vector.y.IsNanOrInfinity() ||
                   vector.z.IsNanOrInfinity()
                ? fallback
                : vector;
        }

        #endregion
    }
}