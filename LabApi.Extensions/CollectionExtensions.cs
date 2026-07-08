using System;
using System.Buffers;
using System.Collections.Generic;

namespace LabApi.Extensions
{
    /// <summary>
    /// Provides high-performance fluent utility extensions for advanced collection mappings and dictionary state engines.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Evaluates a temporal cooldown window for a specific key profile. 
        /// If the tracking node is ready, executes the payload and automatically commits the new timestamp.
        /// </summary>
        /// <typeparam name="TKey">The structural identity lookup key tracking type.</typeparam>
        /// <param name="cooldownMap">The backing storage dictionary tracking execution milestones.</param>
        /// <param name="key">The specific instance identity key target checked for rate limiting.</param>
        /// <param name="window">The required minimum structural <see cref="TimeSpan"/> lock window before the action can rerun.</param>
        /// <param name="throttleAction">The operational delegate invoked seamlessly upon a successful readiness check.</param>
        /// <returns><c>true</c> if the cooldown has successfully elapsed and the action executed; otherwise, <c>false</c>.</returns>
        public static bool ExecuteThrottled<TKey>(this IDictionary<TKey, DateTime> cooldownMap, TKey key, TimeSpan window, Action throttleAction)
        {
            if (cooldownMap is null || throttleAction is null) return false;

            DateTime now = DateTime.UtcNow;

            if (cooldownMap.TryGetValue(key, out DateTime lastExecutionTime))
            {
                if (now - lastExecutionTime < window)
                {
                    return false;
                }
            }

            throttleAction.Invoke();
            cooldownMap[key] = now;
            return true;
        }

        /// <summary>
        /// Systematically purges all historical mapping entries whose tracking timestamps have safely elapsed past a specific temporal comparison index.
        /// Mitigates collection modification exceptions by isolating target records dynamically via zero-allocation memory pooling.
        /// </summary>
        /// <typeparam name="TKey">The structural identity lookup key tracking type.</typeparam>
        /// <param name="dictionary">The target backing storage dictionary undergoing reference cleanup.</param>
        /// <param name="comparisonTime">The core current <see cref="DateTime"/> index acting as the expiration threshold criteria.</param>
        public static void PruneExpired<TKey>(this IDictionary<TKey, DateTime> dictionary, DateTime comparisonTime)
        {
            if (dictionary is null || dictionary.Count == 0) return;

            int totalCount = dictionary.Count;

            // Performance Optimization: Rent contiguous memory from thread-safe ArrayPool to eliminate GC heap allocation spikes
            TKey[] rentedStorage = ArrayPool<TKey>.Shared.Rent(totalCount);
            int expiredCount = 0;

            try
            {
                // Performance Optimization: Pattern match concrete Dictionary to bypass interface dispatch allocation and leverage struct enumerator
                if (dictionary is Dictionary<TKey, DateTime> concreteDict)
                {
                    foreach (KeyValuePair<TKey, DateTime> kvp in concreteDict)
                    {
                        if (comparisonTime >= kvp.Value)
                        {
                            rentedStorage[expiredCount++] = kvp.Key;
                        }
                    }
                }
                else
                {
                    foreach (KeyValuePair<TKey, DateTime> kvp in dictionary)
                    {
                        if (comparisonTime >= kvp.Value)
                        {
                            rentedStorage[expiredCount++] = kvp.Key;
                        }
                    }
                }

                // Mutate the backing collection safely outside the evaluation iteration loop boundary
                for (int i = 0; i < expiredCount; i++)
                {
                    dictionary.Remove(rentedStorage[i]);
                }
            }
            finally
            {
                // Return rented buffer block to shared pool and clear data segments safely to avoid reference leaks
                ArrayPool<TKey>.Shared.Return(rentedStorage, clearArray: true);
            }
        }

        /// <summary>
        /// Evaluates defensively whether a specific key profile is currently locked within an active temporal cooldown window.
        /// </summary>
        /// <typeparam name="TKey">The structural identity lookup key tracking type.</typeparam>
        /// <param name="cooldownMap">The target backing storage dictionary tracking execution milestones.</param>
        /// <param name="key">The specific instance identity key target checked for active cooldown metrics.</param>
        /// <returns><c>true</c> if the tracking node exists and its registered expiration timestamp sits in the future; otherwise, <c>false</c>.</returns>
        public static bool IsCooldownActive<TKey>(this IDictionary<TKey, DateTime> cooldownMap, TKey key)
        {
            if (cooldownMap is null || cooldownMap.Count == 0) return false;

            return cooldownMap.TryGetValue(key, out DateTime expiryTime) && DateTime.UtcNow < expiryTime;
        }

        /// <summary>
        /// Atomically evaluates a temporal gate check for a specific key target. 
        /// If the lock window has elapsed, automatically commits a new future expiration milestone and grants authorization.
        /// </summary>
        /// <typeparam name="TKey">The structural identity lookup key tracking type.</typeparam>
        /// <param name="cooldownMap">The backing storage dictionary tracking execution milestones.</param>
        /// <param name="key">The specific instance identity key target checked for rate limiting.</param>
        /// <param name="lockWindow">The chronological execution lock window duration applied if authorization is granted.</param>
        /// <returns><c>true</c> if the gate was open, authorization was granted, and the new lock was committed; otherwise, <c>false</c>.</returns>
        public static bool TryAcquireLock<TKey>(this IDictionary<TKey, DateTime> cooldownMap, TKey key, TimeSpan lockWindow)
        {
            if (cooldownMap is null) return false;

            DateTime now = DateTime.UtcNow;

            if (cooldownMap.TryGetValue(key, out DateTime nextAllowedTime))
            {
                if (now < nextAllowedTime)
                {
                    return false;
                }
            }

            cooldownMap[key] = now + lockWindow;
            return true;
        }
    }
}