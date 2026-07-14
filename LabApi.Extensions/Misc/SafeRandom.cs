using System;

namespace LabApi.Extensions.Misc
{
    /// <summary>
    /// Thread-safe utility for generating pseudo-random numbers.
    /// Uses ThreadStatic isolation to eliminate concurrency locks across asynchronous coroutines and threads.
    /// </summary>
    public static class SafeRandom
    {
        [ThreadStatic]
        private static Random _localRandom;

        /// <summary>
        /// Gets the thread-isolated Random instance, lazily initializing it with a unique seed.
        /// </summary>
        private static Random ThreadRandom => _localRandom ??= new Random(Guid.NewGuid().GetHashCode());

        /// <summary>
        /// Generates a thread-safe random integer between min (inclusive) and max (exclusive).
        /// </summary>
        public static int Next(int min, int max) => ThreadRandom.Next(min, max);

        /// <summary>
        /// Generates a thread-safe random double between 0.0 and 1.0.
        /// </summary>
        public static double NextDouble() => ThreadRandom.NextDouble();

        /// <summary>
        /// Generates a thread-safe random float between min (inclusive) and max (inclusive).
        /// </summary>
        public static float Range(float min, float max) => (float)(ThreadRandom.NextDouble() * (max - min) + min);
    }
}