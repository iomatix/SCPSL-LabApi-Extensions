using System;

namespace LabApi.Extensions.RoundManagement
{
    /// <summary>
    /// Abstract template for defining custom server-side round ending sequences, gamemode transitions, or map events.
    /// </summary>
    public abstract class RoundScenario
    {
        protected readonly RoundController Controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoundScenario"/> class bound to a central lifecycle controller safely.
        /// </summary>
        protected RoundScenario(RoundController roundController)
        {
            // FIX: Added guard check to guarantee that the scenario is never bound to a null controller.
            Controller = roundController ?? throw new ArgumentNullException(nameof(roundController));
        }

        /// <summary>
        /// Gets or sets an optional custom lifecycle cleanup hook executed automatically when the scenario completes.
        /// </summary>
        public Action OnCleanupRequested { get; set; }

        /// <summary>
        /// Executes the concrete internal scenario logic. Must be overridden by implementation nodes.
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// Finalizes the scenario lifecycle step, automatically invoking registered external subsystem cleanup actions safely.
        /// </summary>
        protected virtual void OnComplete()
        {
            OnCleanupRequested?.Invoke();
        }
    }
}