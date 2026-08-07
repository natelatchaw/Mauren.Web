using Mauren.Discord.Application.Abstractions;
using Mauren.Discord.Application.Features.Configuration;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;

namespace Mauren.Discord.Infrastructure.Plugins
{
    /// <inheritdoc/>
    internal class PluginFileProviderFactory : IPluginFileProviderFactory
    {
        private readonly IOptionsProvider<BotOptions> _optionsProvider;
        private String? ExistingPath { get; set; }
        private IFileProvider? ExistingProvider { get; set; }
        /// <summary>
        /// A lock object for thread safety.
        /// </summary>
        private readonly Object _lock = new();

        public PluginFileProviderFactory(IOptionsProvider<BotOptions> optionsProvider)
        {
            _optionsProvider = optionsProvider;
        }

        /// <inheritdoc/>
        IFileProvider IPluginFileProviderFactory.GetProvider()
        {
            // If the current plugin path is null/whitespace
            if (String.IsNullOrWhiteSpace(_optionsProvider.Current.PluginPath))
            {
                throw new InvalidOperationException("Plugin path is not set in the options provider.");
            }

            // Get the absolute path of the current plugin path (PhysicalFileProvider requires an absolute path)
            String absolutePath = Path.GetFullPath(_optionsProvider.Current.PluginPath);

            // Lock to prevent race conditions during concurrent access
            lock (_lock)
            {
                // If the current path is the same as the existing path and a provider already exists
                if (absolutePath.Equals(ExistingPath, StringComparison.OrdinalIgnoreCase) && ExistingProvider is IFileProvider currentProvider)
                {
                    // Return the existing provider
                    return currentProvider;
                }

                // If the current path does not exist
                if (Directory.Exists(absolutePath) is false)
                {
                    // Create the directory
                    Directory.CreateDirectory(absolutePath);
                }

                // If the existing provider is disposable
                if (ExistingProvider is IDisposable disposableProvider)
                {
                    // Dispose the existing provider
                    disposableProvider.Dispose();
                }

                // Set the existing path to the current path
                ExistingPath = absolutePath;
                // Set the existing provider to a newly constructed provider
                ExistingProvider = new PhysicalFileProvider(absolutePath);

                // Return the provider
                return ExistingProvider;
            }
        }
    }
}
