using Discord.Interactions;
using Mauren.Discord.Application.Abstractions.Modules;
using Mauren.Extensions.Plugins.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Infrastructure.Modules
{
    /// <inheritdoc/>
    internal class ModuleRepository<TContract> : IModuleRepository<TContract>, IDisposable, IModuleRegistry
    {
        private readonly ILogger<ModuleRepository<TContract>> _logger;
        private readonly IPluginLoader<TContract> _pluginLoader;
        private readonly InteractionService _interactionService;

        private readonly ConcurrentDictionary<String, IList<ModuleInfo>> _registry;
        private readonly ConcurrentDictionary<ModuleInfo, IServiceProvider> _providers;

        public ModuleRepository(ILogger<ModuleRepository<TContract>> logger, IPluginLoader<TContract> pluginLoader, 
            InteractionService interactionService)
        {
            _logger = logger;
            _pluginLoader = pluginLoader;
            _interactionService = interactionService;

            _registry = new();
            _providers = new();

            _pluginLoader.PluginLoaded += OnPluginLoaded;
            _pluginLoader.PluginUnloaded += OnPluginUnloaded;
        }

        /// <inheritdoc/>
        void IDisposable.Dispose()
        {
            _pluginLoader.PluginLoaded -= OnPluginLoaded;
            _pluginLoader.PluginUnloaded -= OnPluginUnloaded;
        }

        /// <summary>
        /// Handles the event triggered when a plugin is successfully loaded by the ALC.
        /// </summary>
        private void OnPluginLoaded(Object? sender, IPluginContext<TContract> e)
        {
            IModuleRepository<TContract> moduleRepository = this;
            _ = Task.Run(async () => await moduleRepository.LoadModulesAsync(e).ConfigureAwait(false));
        }

        /// <summary>
        /// Handles the event triggered when a plugin is successfully unloaded from the ALC.
        /// </summary>
        private void OnPluginUnloaded(Object? sender, IPluginContext<TContract> e)
        {
            IModuleRepository<TContract> moduleRepository = this;
            _ = Task.Run(async () => await moduleRepository.UnloadModulesAsync(e).ConfigureAwait(false));
        }

        /// <inheritdoc/>
        async Task IModuleRepository<TContract>.LoadModulesAsync(IPluginContext<TContract> pluginContext, CancellationToken cancellationToken)
        {
            // Construct a collection to track successfully loaded modules for the provided plugin context
            IList<ModuleInfo> loadedModules = [];

            // Iterate over the collection of types from the plugin context
            foreach (Type type in pluginContext.Types)
            {
                // Throw if cancellation is requested
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    // Add the provided type as a module and provide the plugin context's service provider
                    ModuleInfo moduleInfo = await _interactionService.AddModuleAsync(type, pluginContext.Provider)
                        .ConfigureAwait(false);

                    // Add the received module info to the list of loaded modules
                    loadedModules.Add(moduleInfo);

                    // Add the plugin context's service provider to the provider registry
                    _providers.TryAdd(moduleInfo, pluginContext.Provider);
                }
                catch (Exception exception)
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                        _logger.Log(LogLevel.Error, exception, "Failed to add '{type}' from plugin '{pluginId}'", type.Name, pluginContext.Id);
                }
            }

            // Try to add the plugin context's loaded modules to the registry
            if (_registry.TryAdd(pluginContext.Id, loadedModules) is false)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.Log(LogLevel.Error, "Failed to register loaded modules from plugin '{pluginId}'", pluginContext.Id);

                return;
            }

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.Log(LogLevel.Information, "Successfully added {count} modules from plugin '{pluginId}'", loadedModules.Count, pluginContext.Id);
        }

        /// <inheritdoc/>
        async Task IModuleRepository<TContract>.UnloadModulesAsync(IPluginContext<TContract> pluginContext, CancellationToken cancellationToken)
        {
            // Try to remove the plugin context's loaded modules from the registry
            if (_registry.TryRemove(pluginContext.Id, out IList<ModuleInfo>? loadedModules) is false)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.Log(LogLevel.Error, "Failed to unregister loaded modules from plugin '{pluginId}'", pluginContext.Id);

                return;
            }

            // Iterate over the collection of module info instances from the registry
            foreach (ModuleInfo loadedModule in loadedModules)
            {
                // Throw if cancellation is requested
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    // Remove the plugin context's service provider from the provider registry
                    _providers.TryRemove(loadedModule, out IServiceProvider? provider);

                    // Remove the module from the interaction service
                    await _interactionService.RemoveModuleAsync(loadedModule).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                        _logger.Log(LogLevel.Error, exception, "Failed to remove '{module}' from plugin '{pluginId}'", loadedModule.Name, pluginContext.Id);
                }
            }

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.Log(LogLevel.Information, "Successfully removed {count} modules from plugin '{pluginId}'", loadedModules.Count, pluginContext.Id);
        }

        /// <inheritdoc/>
        async Task IModuleRepository.SyncCommandsAsync(UInt64? guildId, CancellationToken cancellationToken)
        {
            // Try to sync commands stored in the interaction service
            try
            {
                // Throw if cancellation is requested before we start
                cancellationToken.ThrowIfCancellationRequested();

                if (guildId.HasValue)
                {
                    await _interactionService.RegisterCommandsToGuildAsync(guildId.Value, deleteMissing: true)
                        .ConfigureAwait(false);
                    
                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.Log(LogLevel.Information, "Successfully synchronized slash commands to guild '{guildId}'.", guildId.Value);
                }
                else
                {
                    // Sync globally (can take up to an hour to cache on Discord's end)
                    await _interactionService.RegisterCommandsGloballyAsync(deleteMissing: true)
                        .ConfigureAwait(false);
                    
                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.Log(LogLevel.Information, "Successfully synchronized slash commands globally.");
                }
            }
            catch (Exception exception)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.Log(LogLevel.Error, exception, "Failed to synchronize commands with the Discord API.");

                // Re-throw
                throw;
            }
        }

        /// <inheritdoc/>
        Task<IServiceProvider> IModuleRegistry.GetServiceProviderAsync(ModuleInfo moduleInfo, CancellationToken cancellationToken)
        {
            // Try to get the module info's service provided from the provider registry
            if (_providers.TryGetValue(moduleInfo, out IServiceProvider? serviceProvider) is false)
                throw new InvalidOperationException($"Could not determine {nameof(IServiceProvider)} for module {moduleInfo.Name}");

            // Return the service provider
            return Task.FromResult(serviceProvider);
        }
    }
}