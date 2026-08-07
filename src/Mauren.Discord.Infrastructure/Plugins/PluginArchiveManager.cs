using Mauren.Discord.Application.Abstractions;
using Mauren.Discord.Application.Abstractions.Plugins;
using Mauren.Discord.Application.Features.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Infrastructure.Plugins
{
    /// <inheritdoc/>
    internal class PluginArchiveManager : IPluginArchiveManager
    {
        private readonly ILogger<PluginArchiveManager> _logger;
        private readonly IOptionsProvider<BotOptions> _optionsProvider;

        public PluginArchiveManager(ILogger<PluginArchiveManager> logger, IOptionsProvider<BotOptions> optionsProvider)
        {
            _logger = logger;
            _optionsProvider = optionsProvider;
        }

        /// <inheritdoc/>
        async Task<String> IPluginArchiveManager.ExtractArchiveAsync(Stream stream, CancellationToken cancellationToken)
        {
            // Get the current path of the plugin directory
            String? currentPath = _optionsProvider.Current.PluginPath;
            // If the plugin directory path is null/whitespace
            if (String.IsNullOrWhiteSpace(currentPath))
                throw new InvalidOperationException($"Plugins directory has not been configured.");

            // Resolve the fully qualified path of the plugin directory
            String absolutePath = Path.GetFullPath(currentPath);
            // If the path does not exist on disk
            if (Directory.Exists(absolutePath) is false)
                Directory.CreateDirectory(absolutePath);

            // Get a random subpath for the extraction destination
            String subpath = Path.GetRandomFileName();
            // Create the fully qualified path of the plugin subdirectory
            String absoluteSubpath = Path.Combine(absolutePath, subpath);

            // Create the extracted plugin's subdirectory
            DirectoryInfo pluginSubdirectory = Directory.CreateDirectory(absoluteSubpath);

            // Construct a new ZipArchive instance from the provided stream
            using ZipArchive archive = new(stream, ZipArchiveMode.Read, leaveOpen: true);

            // Iterate over the collection of zip archive entries
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                // If the entry's name is null/empty, skip the entry
                if (String.IsNullOrEmpty(entry.Name)) continue;

                // Create the path of the entry's destination
                String destinationPath = Path.Combine(pluginSubdirectory.FullName, entry.FullName);
                // Resolve the fully qualified path of the entry's destination
                String absoluteDestinationPath = Path.GetFullPath(destinationPath);
                // If the absolute destination path does not start with the plugin subdirectory path (Zip Slip attack)
                if (absoluteDestinationPath.StartsWith(pluginSubdirectory.FullName, StringComparison.OrdinalIgnoreCase) is false)
                {
                    // Delete the entire plugin subdirectory
                    pluginSubdirectory.Delete(recursive: true);
                    // Throw exception
                    throw new InvalidOperationException($"Zip entry '{entry.Name}' attempts to extract outside the target directory");
                }

                // Get the subpath of the destination
                String? destinationSubpath = Path.GetDirectoryName(absoluteDestinationPath);
                // If the subpath of the destination is not null and does not yet exist
                if (destinationSubpath != null && Directory.Exists(destinationSubpath) is false)
                {
                    // Create the entry's destination subpath
                    Directory.CreateDirectory(destinationSubpath);
                }

                // Open the entry to a stream
                await using Stream entryStream = entry.Open();
                // Create a file stream for the destination file
                await using FileStream fileStream = new(absoluteDestinationPath, FileMode.Create);
                // Copy the entry stream to the file stream
                await entryStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
            }

            // Return the subpath of the extracted plugin
            return subpath;
        }

        /// <inheritdoc/>
        Task IPluginArchiveManager.DeleteArchiveAsync(String subpath, CancellationToken cancellationToken)
        {
            // If the provided subpath is null/whitespace
            ArgumentException.ThrowIfNullOrWhiteSpace(subpath);

            // Get the current path of the plugin directory
            String? currentPath = _optionsProvider.Current.PluginPath;
            // If the plugin directory path is null/whitespace
            if (String.IsNullOrWhiteSpace(currentPath))
                throw new InvalidOperationException($"Plugins directory has not been configured.");

            // Resolve the fully qualified path of the plugin directory
            String absolutePath = Path.GetFullPath(currentPath);
            // If the absolute path does not end with a directory seperator
            if (absolutePath.EndsWith(Path.DirectorySeparatorChar) is false)
                // Append a directory seperator
                absolutePath += Path.DirectorySeparatorChar;

            // Create the fully qualified path of the plugin subdirectory
            String absoluteSubpath = Path.Combine(absolutePath, subpath);

            // Get the full path of the target directory
            String targetDirectory = Path.GetFullPath(absoluteSubpath);
            // If the absolute destination path does not start with the plugin subdirectory path
            if (targetDirectory.StartsWith(absolutePath, StringComparison.OrdinalIgnoreCase) is false)
            {
                // Throw exception
                throw new InvalidOperationException($"Subpath '{subpath}' attempts to delete outside the target directory");
            }

            // If the specified plugin subdirectory exists
            if (Directory.Exists(targetDirectory))
            {
                // Delete the plugin subdirectory recursively
                Directory.Delete(targetDirectory, recursive: true);
            }

            // Return a task indicating completion
            return Task.CompletedTask;
        }
    }
}
