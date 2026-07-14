using LabApi.Features.Wrappers;
using MEC;
using System.Collections.Generic;

namespace LabApi.Extensions.RoundManagement
{
    /// <summary>
    /// Centralized controller to register, track, and safely execute custom round scenarios while overriding native round ending metrics.
    /// </summary>
    public class RoundController
    {
        private readonly HashSet<RoundScenario> _registeredScenarios = new HashSet<RoundScenario>();
        private bool _autoRoundEndLocked;

        /// <summary>
        /// Gets a value indicating whether the native server automatic round evaluation loop is currently locked out.
        /// </summary>
        public bool IsRoundEndLocked => _autoRoundEndLocked;

        /// <summary>
        /// Gets all currently registered round scenarios.
        /// </summary>
        public IEnumerable<RoundScenario> EndingScenarios => _registeredScenarios;

        /// <summary>
        /// Registers a custom round scenario into the internal registry safely.
        /// </summary>
        public void RegisterScenario(RoundScenario scenario)
        {
            // FIX: Unified null-checking standard (== null instead of is null).
            if (scenario != null)
            {
                _registeredScenarios.Add(scenario);
            }
        }

        /// <summary>
        /// Locks or unlocks native automatic round ending routines utilizing underlying LabAPI framework properties.
        /// </summary>
        public void SetAutoRoundEndLock(bool locked)
        {
            _autoRoundEndLocked = locked;
            Round.IsLocked = locked;
        }

        /// <summary>
        /// Enforces an administrative lockout on standard round termination and executes the targeted scenario logic stream immediately.
        /// </summary>
        public void ExecuteScenario(RoundScenario scenario)
        {
            if (scenario == null)
                return;

            SetAutoRoundEndLock(true);
            scenario.Execute();
        }

        /// <summary>
        /// Private lightweight coroutine that handles deferred round termination safely on the stack.
        /// </summary>
        private IEnumerator<float> EndRoundCoroutine(float delay)
        {
            if (delay > 0f)
            {
                yield return Timing.WaitForSeconds(delay);
            }

            SetAutoRoundEndLock(false);
            Round.End(force: true);
        }

        /// <summary>
        /// Gracefully terminates the round lifecycle sequence after a deferred temporal delay window with zero heap allocations.
        /// </summary>
        public void EndRoundGracefully(float delay = 0f, string coroutineTag = "LabApi.RoundManagement-customRoundEnd")
        {
            // FIX: Bypassed MEC's CallDelayed to run our custom lightweight, closure-free coroutine.
            // Spawns with the tag directly to avoid post-spawn race conditions.
            if (string.IsNullOrEmpty(coroutineTag))
                Timing.RunCoroutine(EndRoundCoroutine(delay));
            else
                Timing.RunCoroutine(EndRoundCoroutine(delay), coroutineTag);
        }

        /// <summary>
        /// Queries the registered scenario registry to resolve a specific strongly-typed scenario with zero heap allocations.
        /// </summary>
        public T GetScenario<T>() where T : RoundScenario
        {
            if (_registeredScenarios == null)
                return null;

            // FIX: Replaced memory-allocating LINQ .OfType<T>().FirstOrDefault() with direct zero-allocation foreach.
            // This leverages HashSet's struct enumerator to avoid GC pressure during hot-path lookups.
            foreach (var scenario in _registeredScenarios)
            {
                if (scenario is T target)
                {
                    return target;
                }
            }

            return null;
        }
    }
}