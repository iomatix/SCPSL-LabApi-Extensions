using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using System.Collections.Generic;

namespace LabApi.Extensions.Escape
{
    /// <summary>
    /// Utility engine designed to orchestrate and execute custom player escape scenarios safely.
    /// </summary>
    public static class EscapeEngine
    {
        /// <summary>
        /// Private lightweight helper coroutine to complete a player's role transition after a delay.
        /// Bypasses delegate closure allocations on the heap.
        /// </summary>
        private static IEnumerator<float> CompletePlayerEscapeCoroutine(Player player, float delay)
        {
            if (delay > 0f)
            {
                yield return Timing.WaitForSeconds(delay);
            }

            if (player != null && player.IsReady)
            {
                player.SetRole(RoleTypeId.Spectator, RoleChangeReason.Escaped);
            }
        }

        /// <summary>
        /// Runs a non-blocking asynchronous evacuation routine driven by the provided scenario parameters.
        /// </summary>
        /// <param name="scenario">The escape scenario configuration rules driving this process.</param>
        /// <returns>An enumerator tracking the active timeline states.</returns>
        public static IEnumerator<float> RunScenarioRoutine(EscapeScenario scenario)
        {
            if (scenario == null)
                yield break;

            // Trigger started hooks
            scenario.OnSequenceStarted?.Invoke();

            // Broadcast the initial hint safely
            if (!string.IsNullOrEmpty(scenario.InitialHint))
            {
                PlayerExtensions.BroadcastHintToAll(scenario.InitialHint, scenario.InitialHintDuration);
            }

            // Phase 1: Initial Delay
            yield return Timing.WaitForSeconds(scenario.InitialDelay);

            // Trigger intermediary processing event
            scenario.OnSequenceProcessing?.Invoke();

            // Phase 2: Processing Delay (e.g. waiting for doors or transport vehicles)
            yield return Timing.WaitForSeconds(scenario.ProcessingDelay);

            var players = Player.ReadyList;
            if (players != null)
            {
                // FIX: Optimized loop lookup to completely bypass enumerator allocations where possible.
                if (players is List<Player> list)
                {
                    int count = list.Count;
                    for (int i = 0; i < count; i++)
                    {
                        ProcessPlayerEscape(list[i], scenario);
                    }
                }
                else
                {
                    foreach (Player player in players)
                    {
                        ProcessPlayerEscape(player, scenario);
                    }
                }
            }

            // Terminate sequence cleanly
            scenario.OnSequenceFinalized?.Invoke();
        }

        /// <summary>
        /// Analyzes a player's eligibility and performs the teleportation and effect sequences safely.
        /// </summary>
        private static void ProcessPlayerEscape(Player player, EscapeScenario scenario)
        {
            if (player == null || !player.IsReady || !player.IsAlive || player.IsSCP)
                return;

            // FIX: Integrated our newly optimized, thread-safe squared distance math extension.
            // Bypasses heavy square root calculations and native Unity boundary lookups.
            if (player.Position.IsWithinRange(scenario.EscapeZone, scenario.EscapeRadius))
            {
                // Fire custom plugin logic hooks safely
                scenario.OnPlayerValidated?.Invoke(player);

                // Apply status effects and teleport player
                player.IsGodModeEnabled = true;

                if (!string.IsNullOrEmpty(scenario.SuccessHint))
                {
                    player.SendHint(scenario.SuccessHint);
                }

                player.Position = scenario.TeleportPosition;
                player.ClearInventory();
                player.ApplySensoryShock(scenario.FlashDuration, scenario.BlindDuration, scenario.BlurDuration, scenario.DeafenDuration);

                // FIX: Replaced memory-expensive Timing.CallDelayed with our custom lightweight, closure-free delay routine.
                Timing.RunCoroutine(
                    CompletePlayerEscapeCoroutine(player, scenario.FinalDelay),
                    "LabApi.Escape-playerComplete"
                );
            }
        }
    }
}