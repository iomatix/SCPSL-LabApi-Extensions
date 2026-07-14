using System;
using System.Collections.Generic;

namespace LabApi.Extensions.Misc
{
    /// <summary>
    /// Outlines the structural resolution status of a fuzzy text interpretation sequence.
    /// </summary>
    public enum InterpretationStatus
    {
        DefinitiveMatch,
        Ambiguous,
        NoMatchFound
    }

    /// <summary>
    /// Immutable result container holding the execution metrics of a string interpretation sequence.
    /// Implemented as a readonly struct to prevent heap allocations during parsing.
    /// </summary>
    /// <typeparam name="T">The target structural enum restriction type.</typeparam>
    public readonly struct InterpretationResult<T> where T : struct, Enum
    {
        /// <summary>
        /// Gets the final resolution status of the execution pipeline.
        /// </summary>
        public InterpretationStatus Status { get; }

        /// <summary>
        /// Gets the verified strongly-typed enum value. Evaluates to default if status is not DefinitiveMatch.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Gets the list of close structural candidates discovered if the command invocation was ambiguous.
        /// </summary>
        public string[] Candidates { get; }

        public InterpretationResult(InterpretationStatus status, T value, string[] candidates)
        {
            Status = status;
            Value = value;
            Candidates = candidates ?? Array.Empty<string>();
        }
    }

    /// <summary>
    /// High-performance parsing engine delivering fuzzy string resolution, acronym mapping, and typo-tolerance layers.
    /// Uses static generic caching to completely eliminate runtime reflection overhead.
    /// </summary>
    public static class StringInterpretationExtensions
    {
        /// <summary>
        /// Static generic cache to store enum properties once per type, avoiding heavy runtime reflection and allocations.
        /// </summary>
        private static class EnumCache<T> where T : struct, Enum
        {
            public static readonly string[] Names = Enum.GetNames(typeof(T));
            public static readonly T[] Values = (T[])Enum.GetValues(typeof(T));
            public static readonly string[] LowerNames = Array.ConvertAll(Names, static n => n.ToLowerInvariant());
            public static readonly string[] Acronyms = Array.ConvertAll(Names, static n =>
            {
                int cap = 0;
                for (int i = 0; i < n.Length; i++)
                {
                    if (char.IsUpper(n[i]))
                        cap++;
                }

                if (cap == 0)
                    return string.Empty;

                char[] chars = new char[cap];
                int idx = 0;
                for (int i = 0; i < n.Length; i++)
                {
                    if (char.IsUpper(n[i]))
                    {
                        chars[idx++] = char.ToLowerInvariant(n[i]);
                    }
                }
                return new string(chars);
            });
        }

        /// <summary>
        /// Interprets a raw string input and resolves it against an enum layout using multi-stage heuristic cascades.
        /// </summary>
        public static InterpretationResult<T> InterpretEnum<T>(this string input) where T : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(input))
                return new InterpretationResult<T>(InterpretationStatus.NoMatchFound, default, null);

            string cleanInput = input.Trim().ToLowerInvariant();

            // Retrieve cached enum representations with 0 reflection overhead
            string[] enumNames = EnumCache<T>.Names;
            string[] lowerNames = EnumCache<T>.LowerNames;
            T[] enumValues = EnumCache<T>.Values;
            string[] acronyms = EnumCache<T>.Acronyms;

            // STAGE 1: Exact / Case-Insensitive String Verification
            for (int i = 0; i < enumNames.Length; i++)
            {
                if (lowerNames[i] == cleanInput)
                {
                    return new InterpretationResult<T>(InterpretationStatus.DefinitiveMatch, enumValues[i], null);
                }
            }

            // Pre-size list to avoid resize allocations on hot path
            var candidatesIndexes = new List<int>(enumNames.Length);

            // STAGE 2: Substring Inversion & Structural Acronym Discovery Loop
            for (int i = 0; i < enumNames.Length; i++)
            {
                string nameLower = lowerNames[i];

                // Direct or inverted containment detection (e.g., "Light" matches "LightContainment")
                if (nameLower.Contains(cleanInput) || cleanInput.Contains(nameLower))
                {
                    candidatesIndexes.Add(i);
                    continue;
                }

                // Dynamic uppercase acronym signature matching
                string acronym = acronyms[i];
                if (acronym == cleanInput || (acronym + "z") == cleanInput || acronym == (cleanInput + "z"))
                {
                    candidatesIndexes.Add(i);
                    continue;
                }

                // Hardcoded environmental shortcuts to bulletproof default SCP:SL mapping layouts
                if (cleanInput == "lcz" && nameLower.Contains("light")) { candidatesIndexes.Add(i); continue; }
                if (cleanInput == "hcz" && nameLower.Contains("heavy")) { candidatesIndexes.Add(i); continue; }
                if (cleanInput == "ez" && nameLower.Contains("entrance")) { candidatesIndexes.Add(i); continue; }
                if (cleanInput == "sz" && nameLower.Contains("surface")) { candidatesIndexes.Add(i); continue; }
            }

            // STAGE 3: Typo-Tolerance Matrix via Levenshtein Distance algorithms (Fallback layer)
            if (candidatesIndexes.Count == 0)
            {
                int minDistance = int.MaxValue;

                // FIX: Single-pass loop to find the lowest distance and collect candidates with absolutely 0 LINQ allocations.
                for (int i = 0; i < enumNames.Length; i++)
                {
                    int distance = StringExtensions.ComputeLevenshteinDistance(cleanInput, lowerNames[i]);

                    // Allow soft variations (maximum of 3 typographic mutations allowed based on lengths)
                    if (distance <= 3 && distance < enumNames[i].Length - 2)
                    {
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            candidatesIndexes.Clear();
                            candidatesIndexes.Add(i);
                        }
                        else if (distance == minDistance)
                        {
                            candidatesIndexes.Add(i);
                        }
                    }
                }
            }

            // FIX: Removed redundant candidatesIndexes.Distinct().ToList() allocations. 
            // The logic guarantees uniqueness natively as 'continue' prevents double registration.
            int matchCount = candidatesIndexes.Count;

            if (matchCount == 1)
            {
                return new InterpretationResult<T>(InterpretationStatus.DefinitiveMatch, enumValues[candidatesIndexes[0]], null);
            }

            if (matchCount > 1)
            {
                string[] suggestionStrings = new string[matchCount];
                for (int i = 0; i < matchCount; i++)
                {
                    suggestionStrings[i] = enumNames[candidatesIndexes[i]];
                }
                return new InterpretationResult<T>(InterpretationStatus.Ambiguous, default, suggestionStrings);
            }

            return new InterpretationResult<T>(InterpretationStatus.NoMatchFound, default, null);
        }
    }
}