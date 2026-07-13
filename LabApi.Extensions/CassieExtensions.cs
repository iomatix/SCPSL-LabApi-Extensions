using Cassie;
using LabApi.Extensions.Misc;
using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;

namespace LabApi.Extensions
{
    public static class CassieExtensions
    {
        public static void CassieClear() => Announcer.Clear();

        /// <summary>
        /// Systematically scrubs raw text fields, stripping hidden carriage returns and formatting errors 
        /// while safely preserving empty strings for intentional text muting configurations.
        /// </summary>
        public static string SanitizeCassieString(this string rawMessage)
            => string.IsNullOrWhiteSpace(rawMessage)
                ? string.Empty
                : rawMessage.Replace("\r", "").Replace("\n", " ").Trim();

        #region Single Message Dispatchers
        /// <summary>
        /// Dispatches an intentionally modulated, corrupted vocal sequence across the global audio matrix, 
        /// computing its final playback duration tracking envelope.
        /// </summary>
        public static double DispatchGlitchyMessage(string message, float glitchChance, float jamChance, string glitchifierOutput)
        {
            string sanitizedOutput = glitchifierOutput.SanitizeCassieString();
            if (string.IsNullOrEmpty(sanitizedOutput)) return 0.0;

            try
            {
                CassiePlaybackModifiers playbackModifiers = default;
                playbackModifiers.Pitch = 0.95f;
                Announcer.Message($"pitch_0.95 {sanitizedOutput}", string.Empty, playBackground: false);
                return Announcer.CalculateDuration(sanitizedOutput, playbackModifiers);
            }
            catch (Exception ex)
            {
                iLogger.Error("Cassie.GlitchyMessage", $"Vocal grid runtime suspension anomaly: {ex.Message}");
                return 0.0;
            }
        }

        /// <summary>
        /// Dispatches a clean, high-clarity vocal notification sequence using optimized high-pass pitch metrics across global facility channels.
        /// </summary>
        public static double DispatchMessage(string message)
        {
            string sanitizedMessage = message.SanitizeCassieString();
            if (string.IsNullOrEmpty(sanitizedMessage)) return 0.0;

            try
            {
                CassiePlaybackModifiers playbackModifiers = default;
                playbackModifiers.Pitch = 1.05f;
                Announcer.Message($"pitch_1.05 {sanitizedMessage}", string.Empty, playBackground: false);
                return Announcer.CalculateDuration(sanitizedMessage, playbackModifiers);
            }
            catch (Exception ex)
            {
                iLogger.Error("Cassie.Message", $"Vocal pipeline critical failure: {ex.Message}");
                return 0.0;
            }
        }

        /// <summary>
        /// Executes defensive sanitization, structural sequence formatting, and real-time streaming transmission of CASSIE phrases 
        /// under concrete queue priority matrices, optionally overriding active subtitle layers.
        /// </summary>
        public static void ProcessAndDispatchMessage(string message, string customSubtitles, bool shouldClear, string pitchModifier, float priority, bool disableMessages = false)
        {
            string sanitizedMessage = message.SanitizeCassieString();
            if (string.IsNullOrEmpty(sanitizedMessage)) return;

            if (shouldClear) Announcer.Clear();

            string sanitizedSubtitles = customSubtitles.SanitizeCassieString();
            string processedSubtitles = (!string.IsNullOrEmpty(sanitizedSubtitles) && !disableMessages) ? sanitizedSubtitles : string.Empty;
            string cleanPitch = string.IsNullOrWhiteSpace(pitchModifier) ? "pitch_1.0" : pitchModifier.Trim();

            Announcer.Message($"{cleanPitch} {sanitizedMessage}", customSubtitles: processedSubtitles, priority: priority, playBackground: false);
        }
        #endregion

        #region Batch Message Dispatchers (Zero-Allocation Internal Implementations)
        /// <summary>
        /// Dispatches a collection of messages sequentially using high-performance iteration patterns.
        /// </summary>
        public static void DispatchMessage(IEnumerable<string> messages)
        {
            if (messages is null) return;

            if (messages is List<string> concreteList)
            {
                int count = concreteList.Count;
                for (int i = 0; i < count; i++)
                {
                    DispatchMessage(concreteList[i]);
                }
                return;
            }

            foreach (string message in messages)
            {
                DispatchMessage(message);
            }
        }

        /// <summary>
        /// Dispatches an inline array of messages sequentially.
        /// </summary>
        public static void DispatchMessage(params string[] messages)
        {
            if (messages is null) return;

            int count = messages.Length;
            for (int i = 0; i < count; i++)
            {
                DispatchMessage(messages[i]);
            }
        }
        #endregion

        #region Utilities & Countdown Mechanics
        /// <summary>
        /// Transforms a primitive integer value token into a structured CASSIE vocal countdown phonetic sequence format 
        /// matching the specific operational context tracking threshold boundaries.
        /// </summary>
        public static string ToCassieCountdown(this int notifyTime, string alertContext = "Seconds until Event Detonation")
        {
            string sanitizedContext = alertContext.SanitizeCassieString();
            if (string.IsNullOrEmpty(sanitizedContext)) sanitizedContext = "Seconds";

            return notifyTime switch
            {
                < 5 => $".G3 {notifyTime} .G5",
                >= 5 and <= 20 => $".G3 {notifyTime} Seconds .G5",
                _ => $".G3 {notifyTime} {sanitizedContext} .G5"
            };
        }

        /// <summary>
        /// Simulates the exact runtime execution footprint duration matching a specific message phrase sequence 
        /// under a localized speed modification configuration envelope.
        /// </summary>
        public static double CalculateCassieMessageDuration(string message, double speed = 0.99)
        {
            string sanitizedMessage = message.SanitizeCassieString();
            if (string.IsNullOrEmpty(sanitizedMessage)) return 0.0;

            CassiePlaybackModifiers modifiers = default;
            modifiers.Pitch = (float)speed;
            return Announcer.CalculateDuration(sanitizedMessage, modifiers);
        }
        #endregion

        #region Aggregation Overhead Loops (Zero-Allocation Internal Implementations)
        /// <summary>
        /// Aggregates and computes total durations avoiding allocation-heavy LINQ structure parsing.
        /// </summary>
        public static double CalculateTotalMessagesDurations(IDictionary<string, float> messageSpeedDictionary)
        {
            if (messageSpeedDictionary is null || messageSpeedDictionary.Count == 0) return 0.0;

            double cumulativeDuration = 0.0;
            if (messageSpeedDictionary is Dictionary<string, float> concreteDict)
            {
                foreach (var kvp in concreteDict)
                {
                    cumulativeDuration += CalculateCassieMessageDuration(kvp.Key, kvp.Value);
                }
                return cumulativeDuration;
            }

            foreach (var kvp in messageSpeedDictionary)
            {
                cumulativeDuration += CalculateCassieMessageDuration(kvp.Key, kvp.Value);
            }
            return cumulativeDuration;
        }

        /// <summary>
        /// Aggregates linear collection playback footprints over a zero-allocation sequential iteration map.
        /// </summary>
        public static double CalculateTotalMessagesDurations(IEnumerable<string> messages, float defaultSpeed = 0.99f)
        {
            if (messages is null) return 0.0;

            double cumulativeDuration = 0.0;
            if (messages is List<string> concreteList)
            {
                int count = concreteList.Count;
                for (int i = 0; i < count; i++)
                {
                    cumulativeDuration += CalculateCassieMessageDuration(concreteList[i], defaultSpeed);
                }
                return cumulativeDuration;
            }

            foreach (string message in messages)
            {
                cumulativeDuration += CalculateCassieMessageDuration(message, defaultSpeed);
            }
            return cumulativeDuration;
        }

        /// <summary>
        /// High-performance overload routing parameters sequentially directly into structural loop wrappers.
        /// </summary>
        public static double CalculateTotalMessagesDurations(float defaultSpeed, params string[] messages)
        {
            if (messages is null || messages.Length == 0) return 0.0;

            double cumulativeDuration = 0.0;
            int count = messages.Length;
            for (int i = 0; i < count; i++)
            {
                cumulativeDuration += CalculateCassieMessageDuration(messages[i], defaultSpeed);
            }
            return cumulativeDuration;
        }
        #endregion
    }
}