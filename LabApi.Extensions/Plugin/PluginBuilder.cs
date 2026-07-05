using LabApi.Loader.Features.Configuration;
using LabApi.Loader.Features.Plugins;
using System;

namespace LabApi.Extensions.Plugin
{
    /// <summary>
    /// Non-generic static companion factory providing implicit type-inference for the fluent pipeline.
    /// </summary>
    public static class PluginBuilder
    {
        /// <summary>
        /// Initiates a fluent orchestration builder instance utilizing implicit compiler type inference.
        /// </summary>
        /// <typeparam name="TConfig">The primary configuration type inferred by the compiler context.</typeparam>
        /// <param name="plugin">The live framework plugin context instance.</param>
        /// <returns>A pristine fluent <see cref="PluginBuilder{TConfig}"/> context.</returns>
        public static PluginBuilder<TConfig> Create<TConfig>(Plugin<TConfig> plugin)
            where TConfig : LabApiConfig, new()
        {
            return new PluginBuilder<TConfig>(plugin);
        }
    }

    /// <summary>
    /// A high-performance fluent orchestration builder designed to streamline sub-configuration deployment and module initialization sequences.
    /// </summary>
    /// <typeparam name="TConfig">The primary framework configuration type conforming to <see cref="LabApiConfig"/>.</typeparam>
    public sealed class PluginBuilder<TConfig> where TConfig : LabApiConfig, new()
    {
        private readonly Plugin<TConfig> _plugin;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginBuilder{TConfig}"/> class.
        /// </summary>
        /// <param name="plugin">The live framework plugin context instance.</param>
        public PluginBuilder(Plugin<TConfig> plugin)
        {
            _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        }

        /// <summary>
        /// Dynamically loads, validates, and binds a decentralized sub-configuration file to the plugin ecosystem.
        /// </summary>
        /// <typeparam name="TSubConfig">The target modular sub-configuration class type being loaded.</typeparam>
        /// <param name="fileName">The specific file name literal layout tracked on disk.</param>
        /// <param name="bindAction">The assignment delegate mapping the loaded instance to a plugin property field.</param>
        /// <param name="validationAction">An optional validation delegate executed to enforce runtime boundary metrics.</param>
        /// <returns>The current context to support method chaining.</returns>
        public PluginBuilder<TConfig> BindSubConfig<TSubConfig>(
            string fileName,
            Action<TSubConfig> bindAction,
            Action<TSubConfig> validationAction = null)
            where TSubConfig : class, new()
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
            if (bindAction is null) throw new ArgumentNullException(nameof(bindAction));

            TSubConfig config = _plugin.LoadOrCreateSubConfig(fileName, validationAction);
            bindAction.Invoke(config);

            return this;
        }

        /// <summary>
        /// Executes an initialization routine or boots up a subsystem component as part of the fluent pipeline.
        /// </summary>
        /// <param name="initAction">The delegate housing the initialization payload sequence.</param>
        /// <returns>The current context to support method chaining.</returns>
        public PluginBuilder<TConfig> InitializeModule(Action initAction)
        {
            initAction?.Invoke();
            return this;
        }
    }
}