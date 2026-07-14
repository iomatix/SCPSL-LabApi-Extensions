using LabApi.Loader.Features.Configuration;
using LabApi.Loader.Features.Plugins;
using System;

namespace LabApi.Extensions.Plugin
{
    /// <summary>
    /// Static companion factory providing implicit type-inference for the fluent plugin builder.
    /// </summary>
    public static class PluginBuilder
    {
        /// <summary>
        /// Initiates a fluent builder instance utilizing implicit compiler type inference.
        /// </summary>
        /// <typeparam name="TConfig">The primary configuration type inferred by the compiler.</typeparam>
        /// <param name="plugin">The live plugin instance.</param>
        /// <returns>A zero-allocation fluent <see cref="PluginBuilder{TConfig}"/> struct.</returns>
        public static PluginBuilder<TConfig> Create<TConfig>(Plugin<TConfig> plugin)
            where TConfig : LabApiConfig, new()
        {
            // FIX: Returns a lightweight struct instead of allocating a class instance on the heap.
            return new PluginBuilder<TConfig>(plugin);
        }
    }

    /// <summary>
    /// A zero-allocation, lightweight fluent builder designed to streamline sub-configuration loading and module initialization.
    /// Implemented as a readonly struct to completely eliminate heap garbage during plugin bootstrap sequences.
    /// </summary>
    /// <typeparam name="TConfig">The primary configuration type conforming to <see cref="LabApiConfig"/>.</typeparam>
    public readonly struct PluginBuilder<TConfig> where TConfig : LabApiConfig, new()
    {
        private readonly Plugin<TConfig> _plugin;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginBuilder{TConfig}"/> struct.
        /// </summary>
        /// <param name="plugin">The live plugin instance.</param>
        public PluginBuilder(Plugin<TConfig> plugin)
        {
            _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        }

        /// <summary>
        /// Dynamically loads, validates, and binds a sub-configuration file to a plugin field or property.
        /// </summary>
        /// <typeparam name="TSubConfig">The target sub-configuration class type being loaded.</typeparam>
        /// <param name="fileName">The specific file name on disk (e.g. "settings.yml").</param>
        /// <param name="bindAction">The assignment callback mapping the loaded configuration to a plugin property.</param>
        /// <param name="validationAction">An optional validation callback executed to enforce custom rules.</param>
        /// <returns>The current struct context to support fluent method chaining with zero heap allocations.</returns>
        public PluginBuilder<TConfig> BindSubConfig<TSubConfig>(
            string fileName,
            Action<TSubConfig> bindAction,
            Action<TSubConfig> validationAction = null)
            where TSubConfig : class, new()
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            // FIX: Unified null-checking standard (== null instead of is null).
            if (bindAction == null)
                throw new ArgumentNullException(nameof(bindAction));

            // Protection against default(PluginBuilder) uninitialized struct invocations.
            if (_plugin == null)
                throw new InvalidOperationException("The PluginBuilder has not been initialized with a valid plugin context.");

            // Leverage our newly optimized LoadOrCreateSubConfig extension
            TSubConfig config = _plugin.LoadOrCreateSubConfig(fileName, validationAction);
            bindAction.Invoke(config);

            return this;
        }

        /// <summary>
        /// Executes an initialization routine or boots up a subsystem component as part of the fluent pipeline.
        /// </summary>
        /// <param name="initAction">The callback containing the initialization steps.</param>
        /// <returns>The current struct context to support fluent method chaining with zero heap allocations.</returns>
        public PluginBuilder<TConfig> InitializeModule(Action initAction)
        {
            initAction?.Invoke();
            return this;
        }
    }
}