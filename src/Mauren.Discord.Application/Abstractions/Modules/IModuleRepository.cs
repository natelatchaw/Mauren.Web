using Mauren.Extensions.Plugins.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Application.Abstractions.Modules
{
    /// <summary>
    /// Defines a non-generic contract for managing the lifecycle of execution modules, 
    /// allowing the application layer to interact with the repository without referencing infrastructure-specific types.
    /// </summary>
    public interface IModuleRepository
    {
        /// <summary>
        /// Pushes the currently loaded interaction modules to the Discord API.
        /// </summary>
        /// 
        /// <param name="guildId">
        /// The optional unique identifier of a specific Discord guild (server). 
        /// If provided, commands are registered exclusively to that guild (which updates instantly). 
        /// If <see langword="null"/>, commands are registered globally across all guilds.
        /// </param>
        /// 
        /// <param name="cancellationToken">
        /// A token to monitor for cancellation requests.
        /// </param>
        /// 
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous sync operation.
        /// </returns>
        Task SyncCommandsAsync(UInt64? guildId = null, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// A generic extension of <see cref="IModuleRepository"/> used to discover and register 
    /// modules of a specific contract type.
    /// </summary>
    /// 
    /// <typeparam name="TContract">
    /// The base contract type that the modules must implement to be discovered and registered.
    /// </typeparam>
    public interface IModuleRepository<TContract> : IModuleRepository
    {
        /// <summary>
        /// Asynchronously discovers, registers, and tracks modules from the provided 
        /// <see cref="IPluginContext{TContract}"/>.
        /// </summary>
        /// 
        /// <param name="pluginContext">
        /// The isolated plugin context containing the discovered types and scoped service provider.
        /// </param>
        /// 
        /// <param name="cancellationToken">
        /// A token to monitor for cancellation requests.
        /// </param>
        /// 
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous load and registration operation.
        /// </returns>
        Task LoadModulesAsync(IPluginContext<TContract> pluginContext, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously unregisters and cleans up modules associated with the provided 
        /// <see cref="IPluginContext{TContract}"/>.
        /// </summary>
        /// 
        /// <param name="pluginContext">
        /// The plugin context whose associated modules should be removed from the host.
        /// </param>
        /// 
        /// <param name="cancellationToken">
        /// A token to monitor for cancellation requests.
        /// </param>
        /// 
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous unload and cleanup operation.
        /// </returns>
        Task UnloadModulesAsync(IPluginContext<TContract> pluginContext, CancellationToken cancellationToken = default);
    }
}
