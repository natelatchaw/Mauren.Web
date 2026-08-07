using Mauren.Discord.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Application.Abstractions.Plugins
{
    /// <summary>
    /// Defines a contract for managing the discovery, loading, unloading, and metadata 
    /// retrieval of physical plugin files within the virtual file system.
    /// </summary>
    public interface IPluginRepository
    {
        /// <summary>
        /// Asynchronously discovers and loads plugin assemblies located at the specified virtual directory subpath.
        /// </summary>
        /// 
        /// <param name="subpath">
        /// The virtual directory subpath to scan for plugin files (e.g., assemblies ending in .dll).
        /// </param>
        /// <param name="cancellationToken">
        /// A token to monitor for cancellation requests.
        /// </param>
        /// 
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous load operation.
        /// </returns>
        Task LoadAsync(String subpath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously unloads previously loaded plugin assemblies located at the specified virtual directory subpath.
        /// </summary>
        /// 
        /// <param name="subpath">
        /// The virtual directory subpath of the plugins to unload.
        /// </param>
        /// <param name="cancellationToken">
        /// A token to monitor for cancellation requests.
        /// </param>
        /// 
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous unload operation.
        /// </returns>
        Task UnloadAsync(String subpath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Scans the root plugin directory and all immediate subdirectories, loading any discovered plugin assemblies into memory.
        /// </summary>
        /// 
        /// <param name="cancellationToken">
        /// A token to monitor for cancellation requests.
        /// </param>
        /// 
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous initialization operation.
        /// </returns>
        Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously unloads all tracked plugins currently residing in memory, 
        /// completely clearing the repository's internal registry.
        /// </summary>
        /// 
        /// <param name="cancellationToken">
        /// A token to monitor for cancellation requests.
        /// </param>
        /// 
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous unload operation.
        /// </returns>
        Task UnloadAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the <see cref="PluginMetadata"/> (such as the parsed manifest and icon) for a specific loaded plugin.
        /// </summary>
        /// 
        /// <param name="pluginId">
        /// The unique identifier of the previously loaded plugin context.
        /// </param>
        /// 
        /// <param name="cancellationToken">
        /// A token to monitor for cancellation requests.
        /// </param>
        /// 
        /// <returns>
        /// A <see cref="Task{PluginMetadata}"/> representing the asynchronous operation, 
        /// containing the metadata for the requested plugin.
        /// </returns>
        /// 
        /// <exception cref="InvalidOperationException">
        /// Thrown when the provided plugin identifier cannot be found in the repository's registry.
        /// </exception>
        Task<PluginMetadata> GetMetadata(String pluginId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the <see cref="PluginMetadata"/> (such as the parsed manifest and icon) 
        /// for all plugins currently loaded and tracked within the repository's internal registry.
        /// </summary>
        /// 
        /// <param name="cancellationToken">
        /// A token to monitor for cancellation requests.
        /// </param>
        /// 
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation, 
        /// containing an enumerable collection of metadata for all active plugins.
        /// </returns>
        Task<IEnumerable<PluginMetadata>> GetAllMetadata(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// A generic extension of <see cref="IPluginRepository"/> used to strictly align the repository 
    /// with a specific plugin contract type within the dependency injection container.
    /// </summary>
    /// 
    /// <typeparam name="TContract">
    /// The base contract type that the managed plugins are expected to implement.
    /// </typeparam>
    public interface IPluginRepository<TContract> : IPluginRepository { }
}
