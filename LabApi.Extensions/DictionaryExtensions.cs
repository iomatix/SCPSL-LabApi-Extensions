using System;
using System.Collections.Generic;

namespace LabApi.Extensions
{
    /// <summary>
    /// Performance-focused dictionary extensions backporting modern .NET features to .NET Framework 4.8.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Backports KeyValuePair deconstruction to .NET Framework 4.8, enabling C# 9.0 pattern matching and tuple-like iteration.
        /// </summary>
        /// <example>
        /// foreach (var (key, value) in myDictionary) { ... }
        /// </example>
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }

        /// <summary>
        /// Gets a value from the dictionary if it exists; otherwise, adds a new value using the provided factory and returns it.
        /// </summary>
        public static TValue GetOrAdd<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TKey, TValue> valueFactory)
        {
            if (dictionary is null)
                throw new ArgumentNullException(nameof(dictionary));
            if (valueFactory is null)
                throw new ArgumentNullException(nameof(valueFactory));

            if (dictionary.TryGetValue(key, out TValue value))
                return value;

            TValue newValue = valueFactory(key);
            dictionary[key] = newValue;
            return newValue;
        }

        /// <summary>
        /// Gets a value from the dictionary if it exists; otherwise, adds a new value using a state-passing factory 
        /// to completely avoid GC closure (lambda display class) allocations.
        /// </summary>
        /// <remarks>
        /// Highly recommended for performance-critical systems inside loops or event handlers.
        /// </remarks>
        public static TValue GetOrAdd<TKey, TValue, TState>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TKey, TState, TValue> valueFactory,
            TState state)
        {
            if (dictionary is null)
                throw new ArgumentNullException(nameof(dictionary));
            if (valueFactory is null)
                throw new ArgumentNullException(nameof(valueFactory));

            if (dictionary.TryGetValue(key, out TValue value))
                return value;

            TValue newValue = valueFactory(key, state);
            dictionary[key] = newValue;
            return newValue;
        }

        /// <summary>
        /// Backports the modern TryRemove / Remove-with-out functionality to .NET Framework 4.8 dictionaries.
        /// Safely attempts to remove a key and outputs the removed value.
        /// </summary>
        /// <returns><c>true</c> if the element is successfully found and removed; otherwise, <c>false</c>.</returns>
        public static bool TryRemove<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, out TValue value)
        {
            if (dictionary is null)
                throw new ArgumentNullException(nameof(dictionary));

            if (dictionary.TryGetValue(key, out value))
            {
                dictionary.Remove(key);
                return true;
            }

            value = default;
            return false;
        }
    }
}