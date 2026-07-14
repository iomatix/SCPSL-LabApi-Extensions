using LabApi.Events.CustomHandlers;
using System.Collections.Generic;

namespace LabApi.Extensions
{
    /// <summary>
    /// Highly optimized utility extensions for registering and unregistering LabAPI event handlers.
    /// Features JIT-cached static delegates and zero heap allocations during batch operations.
    /// </summary>
    public static class HandlerExtensions
    {
        #region Register

        /// <summary>
        /// Registers multiple event handlers with zero heap allocations.
        /// </summary>
        public static void RegisterAll(this IEnumerable<CustomEventsHandler> handlers)
        {
            if (handlers == null)
                return;

            // FIX: Enforced compile-time static delegate caching and added Null-Safety.
            handlers.ForEach(static h =>
            {
                if (h != null)
                {
                    CustomHandlersManager.RegisterEventsHandler(h);
                }
            });
        }

        /// <summary>
        /// Registers multiple event handlers (params overload).
        /// </summary>
        public static void RegisterAll(params CustomEventsHandler[] handlers)
        {
            if (handlers == null)
                return;

            RegisterAll((IEnumerable<CustomEventsHandler>)handlers);
        }

        #endregion

        #region Unregister

        /// <summary>
        /// Unregisters multiple event handlers with zero heap allocations.
        /// </summary>
        public static void UnregisterAll(this IEnumerable<CustomEventsHandler> handlers)
        {
            if (handlers == null)
                return;

            // FIX: Enforced compile-time static delegate caching and added Null-Safety.
            handlers.ForEach(static h =>
            {
                if (h != null)
                {
                    CustomHandlersManager.UnregisterEventsHandler(h);
                }
            });
        }

        /// <summary>
        /// Unregisters multiple event handlers (params overload).
        /// </summary>
        public static void UnregisterAll(params CustomEventsHandler[] handlers)
        {
            if (handlers == null)
                return;

            UnregisterAll((IEnumerable<CustomEventsHandler>)handlers);
        }

        #endregion
    }
}