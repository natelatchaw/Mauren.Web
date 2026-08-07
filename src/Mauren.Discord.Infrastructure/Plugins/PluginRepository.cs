using Mauren.Discord.Application.Abstractions;
using Mauren.Discord.Application.Abstractions.Plugins;
using Mauren.Discord.Core;
using Mauren.Extensions.Plugins.Abstractions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Infrastructure.Plugins
{
    /// <inheritdoc/>
    internal class PluginRepository<TContract> : IPluginRepository<TContract>
    {
        private readonly ILogger<PluginRepository<TContract>> _logger;
        private readonly IPluginFileProviderFactory _fileProviderFactory;
        private readonly IPluginLoader<TContract> _pluginLoader;
        private readonly ConcurrentDictionary<IFileInfo, PluginRegistration> _registry;
        private readonly JsonSerializerOptions _serializerOptions;

        public PluginRepository(ILogger<PluginRepository<TContract>> logger, IPluginFileProviderFactory fileProviderFactory,
            IPluginLoader<TContract> pluginLoader)
        {
            _logger = logger;
            _fileProviderFactory = fileProviderFactory;
            _pluginLoader = pluginLoader;

            _registry = new();
            _serializerOptions = new()
            {
                PropertyNameCaseInsensitive = true,
            };
        }

        /// <inheritdoc/>
        async Task IPluginRepository.LoadAsync(String subpath, CancellationToken cancellationToken)
        {
            // Get the current file provider from the factory
            IFileProvider fileProvider = _fileProviderFactory.GetProvider();

            // Enumerate the directory at the provided subpath
            IDirectoryContents contents = fileProvider.GetDirectoryContents(subpath);

            // Enumerate all DLL files in the provided directory
            IEnumerable<IFileInfo> files = contents
                // Filter to non-directory items
                .Where((IFileInfo fileInfo) => fileInfo.IsDirectory is false)
                // Filter to files that have an extension
                .Where((IFileInfo fileInfo) => fileInfo.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase));

            // Iterate over enumerated files
            foreach (IFileInfo file in files)
            {
                try
                {
                    // Load the file as a plugin and retrieve the plugin context identifier
                    String pluginId = await _pluginLoader.LoadAsync(file, cancellationToken).ConfigureAwait(false);

                    // Create a new registration for the loaded plugin
                    PluginRegistration registration = new() { Id = pluginId, Subpath = subpath, FileInfo = file };

                    // Try to add the plugin registration by its file info
                    _registry.TryAdd(file, registration);
                }
                catch (Exception exception)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.Log(LogLevel.Debug, exception, "Load failed for file '{file}'", file.Name);
                }
            }
        }

        /// <inheritdoc/>
        async Task IPluginRepository.UnloadAsync(String subpath, CancellationToken cancellationToken)
        {
            // Enumerate all registered files that match the provided subpath
            IEnumerable<IFileInfo> files = _registry
                // Filter to entries where the plugin registration's subpath matches the provided subpath
                .Where((KeyValuePair<IFileInfo, PluginRegistration> entry) => entry.Value.Subpath.Equals(subpath, StringComparison.OrdinalIgnoreCase))
                // Map each entry to its key
                .Select((KeyValuePair<IFileInfo, PluginRegistration> entry) => entry.Key);

            // Iterate over enumerated files
            foreach (IFileInfo file in files)
            {
                try
                {
                    // Remove the entry from the registry using its exact key
                    if (_registry.TryRemove(file, out PluginRegistration? registration))
                    {
                        // If the found plugin registration's identifier is null, skip it
                        if (registration?.Id is not String pluginId) continue;

                        // Unload the plugin context by its identifier
                        await _pluginLoader.UnloadAsync(pluginId, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (Exception exception)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.Log(LogLevel.Debug, exception, "Unload failed for registration file '{file}'", file.Name);
                }
            }
        }

        /// <inheritdoc/>
        async Task IPluginRepository.InitializeAsync(CancellationToken cancellationToken)
        {
            // Get the current file provider from the factory
            IFileProvider fileProvider = _fileProviderFactory.GetProvider();

            // Enumerate the root directory
            IDirectoryContents contents = fileProvider.GetDirectoryContents(String.Empty);

            // Get the concrete implementation as the interface
            IPluginRepository pluginRepository = this;

            // Load any plugins sitting directly in the root folder (subpath = "")
            await pluginRepository.LoadAsync(String.Empty, cancellationToken).ConfigureAwait(false);

            // Iterate over all subdirectories (where extracted plugins live)
            IEnumerable<IFileInfo> directories = contents.Where((IFileInfo fileInfo) => fileInfo.IsDirectory);

            // Iterate over the collection of subdirectories
            foreach (IFileInfo directory in directories)
            {
                try
                {
                    // Load the plugins found inside the subdirectory using its folder name as the subpath
                    await pluginRepository.LoadAsync(directory.Name, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.Log(LogLevel.Debug, exception, "Failed to initialize plugins in subdirectory '{directory}'", directory.Name);
                }
            }
        }

        /// <inheritdoc/>
        async Task IPluginRepository.UnloadAllAsync(CancellationToken cancellationToken)
        {
            // Iterate over the collection of registered files
            foreach (IFileInfo file in _registry.Keys)
            {
                try
                {
                    // Try to remove the plugin registration by its file info
                    _registry.TryRemove(file, out PluginRegistration? registration);

                    // If the found plugin registration's identifier is null, skip it
                    if (registration?.Id is not String pluginId) continue;

                    // Unload the plugin context by its identifier
                    await _pluginLoader.UnloadAsync(pluginId, cancellationToken);
                }
                catch (Exception exception)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.Log(LogLevel.Debug, exception, "Unload failed for file '{file}'", file.Name);
                }
            }
        }

        /// <inheritdoc/>
        async Task<PluginMetadata> IPluginRepository.GetMetadata(String pluginId, CancellationToken cancellationToken)
        {
            // Query for the entry matching the provided plugin context identifier
            KeyValuePair<IFileInfo, PluginRegistration>? entry = _registry
                .Where((KeyValuePair<IFileInfo, PluginRegistration> entry) => entry.Value.Id.Equals(pluginId))
                .Cast<KeyValuePair<IFileInfo, PluginRegistration>?>()
                .SingleOrDefault();

            // If the entry was missing or invalid
            if (entry.HasValue is false)
                throw new InvalidOperationException($"Lookup failed for plugin '{pluginId}'");

            // Get the current file provider from the factory
            IFileProvider fileProvider = _fileProviderFactory.GetProvider();

            // Get the file from the entry
            IFileInfo file = entry.Value.Key;

            // Get the plugin registration from the entry
            PluginRegistration registration = entry.Value.Value;

            // If the entry's subpath is null/empty
            if (String.IsNullOrWhiteSpace(registration.Subpath))
                throw new InvalidOperationException($"Invalid subpath provided for plugin '{pluginId}'");

            // Initialize a plugin metadata instance
            PluginMetadata metadata = new()
            {
                Id = registration.Id,
                Subpath = registration.Subpath,
            };

            // Get the expected path of the manifest JSON file
            String manifestPath = String.Join('/', registration.Subpath, "manifest.json");
            // Get the file information for the manifest JSON file
            IFileInfo manifestFile = fileProvider.GetFileInfo(manifestPath);
            // If the manifest JSON file exists
            if (manifestFile.Exists)
            {
                // Try to deserialize the manifest file
                try
                {
                    // Open a read stream for the manifest JSON file
                    await using Stream stream = manifestFile.CreateReadStream();
                    // Deserialize the manifest file stream to a plugin manifest instance
                    PluginManifest? manifest = await JsonSerializer
                        .DeserializeAsync<PluginManifest>(stream, _serializerOptions, cancellationToken)
                        .ConfigureAwait(false);
                    // Set the plugin metadata instance's manifest property
                    metadata.Manifest = manifest;
                }
                catch (Exception exception)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.Log(LogLevel.Debug, exception, "Failed to parse manifest for plugin '{pluginId}'", pluginId);
                }
            }

            // Get the expected path of the icon PNG file
            String iconPath = String.Join('/', registration.Subpath, "icon.png");
            // Get the file information for the icon PNG file
            IFileInfo iconFile = fileProvider.GetFileInfo(iconPath);
            // Set the plugin metadata instance's icon property
            metadata.Icon = iconFile.Exists switch
            {
                true => iconFile,
                false => null,
            };

            return metadata;
        }

        /// <inheritdoc/>
        async Task<IEnumerable<PluginMetadata>> IPluginRepository.GetAllMetadata(CancellationToken cancellationToken)
        {
            // Construct a collection to store retrieved plugin metadata
            IList<PluginMetadata> results = [];

            // Iterate over the stored registry values
            foreach (PluginRegistration registration in _registry.Values)
            {
                // If the registration does not contain the plugin context identifier, skip it
                if (registration.Id is not String pluginId) continue;

                try
                {
                    // Get the concrete implementation as the interface
                    IPluginRepository pluginRepository = this;

                    // Delegate to the singular metadata retrieval method
                    PluginMetadata pluginMetadata = await pluginRepository.GetMetadata(pluginId, cancellationToken).ConfigureAwait(false);
                    // Add the retrieved metadata to the collection
                    results.Add(pluginMetadata);
                }
                catch (Exception exception)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.Log(LogLevel.Debug, exception, "Failed to retrieve metadata for plugin '{pluginId}' during bulk fetch", pluginId);
                }
            }

            // Return plugin metadata collection
            return results;
        }
    }
}
