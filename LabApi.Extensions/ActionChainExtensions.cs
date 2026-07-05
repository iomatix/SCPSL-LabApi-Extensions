using LabApi.Features.Wrappers;
using MEC;
using System;
using System.Collections.Generic;

namespace LabApi.Extensions
{
    /// <summary>
    /// Core data token representation of a sequence node inside a fluent action orchestration pipeline.
    /// </summary>
    internal struct ChainNode<T>
    {
        public Action<T> Callback { get; }
        public float DelayDuration { get; }
        public bool IsDelay { get; }

        public ChainNode(Action<T> callback)
        {
            Callback = callback;
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
    /// A high-performance fluent orchestration builder designed to execute chained asynchronous actions with precise delay thresholds.
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
        /// Enqueues an instantaneous execution step into the action chain pipeline.
        /// </summary>
        public ActionChain<T> Then(Action<T> action)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            _nodes.Add(new ChainNode<T>(action));
            return this;
        }

        /// <summary>
        /// Enqueues a temporal delay standby constraint into the action chain pipeline.
        /// </summary>
        public ActionChain<T> Wait(float seconds)
        {
            if (seconds > 0f)
            {
                _nodes.Add(new ChainNode<T>(seconds));
            }
            return this;
        }

        /// <summary>
        /// Launches the compiled action execution chain sequence as a non-blocking asynchronous coroutine stream.
        /// </summary>
        /// <param name="coroutineTag">An optional string tracking tag identifier used to handle premature eviction.</param>
        /// <returns>A live handle tracking the background execution routine thread.</returns>
        public CoroutineHandle Run(string coroutineTag = null)
        {
            CoroutineHandle handle = Timing.RunCoroutine(ExecutePipelineRoutine());
            if (!string.IsNullOrEmpty(coroutineTag))
            {
                handle.Tag = coroutineTag;
            }
            return handle;
        }

        private IEnumerator<float> ExecutePipelineRoutine()
        {
            foreach (var node in _nodes)
            {
                // Defensive Reference Check: Instantly aborts the execution tree if the target object is evicted from memory mid-chain
                if (_target is Player player && !player.IsReady) yield break;
                if (_target is null) yield break;

                if (node.IsDelay)
                {
                    yield return Timing.WaitForSeconds(node.DelayDuration);
                }
                else
                {
                    node.Callback?.Invoke(_target);
                }
            }
        }
    }

    /// <summary>
    /// Companion factory extensions bringing the fluent <see cref="ActionChain{T}"/> initialization into global target scope.
    /// </summary>
    public static class ActionChainExtensions
    {
        /// <summary>
        /// Initiates a clean, type-safe, fluent action orchestration chain bound to this target instance context.
        /// </summary>
        public static ActionChain<T> CreateChain<T>(this T target) where T : class
        {
            return new ActionChain<T>(target);
        }
    }
}