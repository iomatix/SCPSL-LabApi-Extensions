using LabApi.Features.Wrappers;
using MEC;
using System;
using System.Collections.Generic;

namespace LabApi.Extensions
{
    /// <summary>
    /// Represents a single step in an action chain: either a callback or a delay.
    /// </summary>
    internal readonly struct ChainNode<T>
    {
        public Action<T> Callback { get; }
        public float DelayDuration { get; }
        public bool IsDelay { get; }

        public ChainNode(Action<T> callback)
        {
            Callback = callback ?? throw new ArgumentNullException(nameof(callback));
            DelayDuration = 0f;
            IsDelay = false;
        }

        public ChainNode(float delayDuration)
        {
            Callback = null;
            DelayDuration = delayDuration;
            IsDelay = true;
        }
    }

    /// <summary>
    /// Builds and runs a sequence of actions with optional delays.
    /// </summary>
    public sealed class ActionChain<T> where T : class
    {
        private readonly T _target;
        private readonly List<ChainNode<T>> _nodes = new();

        public ActionChain(T target)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
        }

        /// <summary>
        /// Adds an action to run immediately.
        /// </summary>
        public ActionChain<T> Then(Action<T> action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            _nodes.Add(new ChainNode<T>(action));
            return this;
        }

        /// <summary>
        /// Adds a delay before the next action.
        /// </summary>
        public ActionChain<T> Wait(float seconds)
        {
            if (seconds > 0f)
                _nodes.Add(new ChainNode<T>(seconds));

            return this;
        }

        /// <summary>
        /// Starts executing the chain as a coroutine.
        /// </summary>
        /// <param name="coroutineTag">Optional tag for managing the coroutine.</param>
        public CoroutineHandle Run(string coroutineTag = null)
        {
            // FIX: Register the tag directly in RunCoroutine so MEC indexes it correctly.
            // This prevents issues where Timing.KillCoroutines(tag) fails to find this coroutine.
            return !string.IsNullOrEmpty(coroutineTag)
                ? Timing.RunCoroutine(ExecutePipelineRoutine(), coroutineTag)
                : Timing.RunCoroutine(ExecutePipelineRoutine());
        }

        private IEnumerator<float> ExecutePipelineRoutine()
        {
            foreach (var node in _nodes)
            {
                // FIX: Verify if the target is still valid in the Unity/LabApi context.
                if (!IsTargetValid())
                    yield break;

                if (node.IsDelay)
                {
                    // OPTIMIZATION: Yielding a direct float is the most efficient way to delay in MEC.
                    yield return node.DelayDuration;
                }
                else
                {
                    node.Callback?.Invoke(_target);
                }
            }
        }

        /// <summary>
        /// Performs an advanced state validation of the target object.
        /// </summary>
        private bool IsTargetValid()
        {
            if (_target is Player player)
                return player.IsReady;

            if (_target is UnityEngine.Object unityObject)
                return unityObject != null;

            return _target != null;
        }
    }

    /// <summary>
    /// Creates a new action chain for the target.
    /// </summary>
    public static class ActionChainExtensions
    {
        public static ActionChain<T> CreateChain<T>(this T target) where T : class
            => new ActionChain<T>(target);
    }
}