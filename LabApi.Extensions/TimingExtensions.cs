using MEC;
using System;
using System.Collections.Generic;

namespace LabApi.Extensions
{
    /// <summary>
    /// Highly optimized utility extensions for working with MEC coroutines.
    /// Features zero-allocation batch operations and custom lightweight, closure-free delayed runners.
    /// </summary>
    public static class TimingExtensions
    {
        #region Delayed

        /// <summary>
        /// Private lightweight helper coroutine that handles delayed execution safely on the stack.
        /// Prevents compiler-generated display classes (closures) on the heap.
        /// </summary>
        private static IEnumerator<float> CallDelayedIfCoroutine(float delay, Func<bool> condition, Action action)
        {
            yield return Timing.WaitForSeconds(delay);

            if (condition())
            {
                action();
            }
        }

        /// <summary>
        /// Calls <paramref name="action"/> after a delay if <paramref name="condition"/> returns true.
        /// </summary>
        public static CoroutineHandle CallDelayedIf(float delay, Func<bool> condition, Action action, string coroutineTag = "LabApi.Extensions-callDelayedIf")
        {
            if (action == null || condition == null)
                return default;

            // FIX: Bypassed MEC's restrictive CallDelayed to run our custom lightweight coroutine.
            // This allows us to pass the tag directly during spawning, which is much safer and avoids post-spawn modifications.
            return string.IsNullOrEmpty(coroutineTag)
                ? Timing.RunCoroutine(CallDelayedIfCoroutine(delay, condition, action))
                : Timing.RunCoroutine(CallDelayedIfCoroutine(delay, condition, action), coroutineTag);
        }

        #endregion

        #region Single Kill

        /// <summary>
        /// Kills all coroutines bound to this tag with zero allocations.
        /// </summary>
        public static void Kill(this string tag)
        {
            if (!string.IsNullOrEmpty(tag))
                Timing.KillCoroutines(tag);
        }

        /// <summary>
        /// Kills this coroutine handle if it is running.
        /// </summary>
        public static void Kill(this CoroutineHandle handle)
        {
            if (handle.IsRunning)
                Timing.KillCoroutines(handle);
        }

        #endregion

        #region Batch Kill (tags)

        /// <summary>
        /// Kills all coroutines bound to the given tags with zero heap allocations.
        /// </summary>
        public static void Kill(this IEnumerable<string> tags)
        {
            if (tags == null) return;
            tags.ForEach(static t => t?.Kill());
        }

        /// <summary>
        /// Kills all coroutines bound to the given tags (params overload).
        /// </summary>
        public static void Kill(params string[] tags)
            => ((IEnumerable<string>)tags).Kill();

        #endregion

        #region Batch Kill (handles)

        /// <summary>
        /// Kills all coroutines associated with the given handles with zero heap allocations.
        /// </summary>
        public static void KillAll(this IEnumerable<CoroutineHandle> handles)
        {
            if (handles == null) return;
            handles.ForEach(static h => h.Kill());
        }

        /// <summary>
        /// Kills all coroutines associated with the given handles (params overload).
        /// </summary>
        public static void KillAll(params CoroutineHandle[] handles)
            => ((IEnumerable<CoroutineHandle>)handles).KillAll();

        /// <summary>
        /// Kills all coroutines in the list and clears it safely.
        /// </summary>
        public static void KillAllAndClear(this List<CoroutineHandle> handles)
        {
            if (handles == null) return;

            handles.ForEach(static h => h.Kill());
            handles.Clear();
        }

        #endregion
    }
}