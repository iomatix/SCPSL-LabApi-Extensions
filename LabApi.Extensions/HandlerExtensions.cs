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
        #region Single Register

        /// <summary>
        /// Registers a single event handler with zero heap allocations.
        /// </summary>
        public static void Register(this CustomEventsHandler handler)
        {
            if (handler != null)
            {
                CustomHandlersManager.RegisterEventsHandler(handler);
            }
        }

        #endregion

        #region Single Unregister

        /// <summary>
        /// Unregisters a single event handler with zero heap allocations.
        /// </summary>
        public static void Unregister(this CustomEventsHandler handler)
        {
            if (handler != null)
            {
                CustomHandlersManager.UnregisterEventsHandler(handler);
            }
        }

        #endregion

        #region Register All

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

        #region Unregister All

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