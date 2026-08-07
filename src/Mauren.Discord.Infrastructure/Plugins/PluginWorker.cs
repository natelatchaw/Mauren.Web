using Mauren.Discord.Application.Abstractions;
using Mauren.Discord.Application.Abstractions.Modules;
using Mauren.Discord.Application.Abstractions.Plugins;
using Mauren.Discord.Application.Features.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mauren.Discord.Infrastructure.Plugins
{
    internal class PluginWorker<TContract> : BackgroundService
    {
        private readonly ILogger<PluginWorker<TContract>> _logger;
        private readonly IOptionsProvider<BotOptions> _optionsProvider;
        private readonly IPluginRepository<TContract> _pluginRepository;
        private readonly IModuleRepository<TContract> _moduleRepository;

        private readonly Channel<String> _pluginPathChannel;
        private readonly PluginDirectoryObserver _pluginDirectoryObserver;

        public PluginWorker(ILogger<PluginWorker<TContract>> logger, IOptionsProvider<BotOptions> optionsProvider,
            IPluginRepository<TContract> pluginRepository, IModuleRepository<TContract> moduleRepository)
        {
            _logger = logger;
            _optionsProvider = optionsProvider;
            _moduleRepository = moduleRepository;
            _pluginRepository = pluginRepository;

            _pluginPathChannel = Channel.CreateUnbounded<String>(new UnboundedChannelOptions
            {
                SingleWriter = false,
                SingleReader = true,
            });
            _pluginDirectoryObserver = new(_logger, _pluginPathChannel.Writer);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Subscribe to the token observer to receive plugin directory changes from the options provider
            using IDisposable subscription = _optionsProvider.Subscribe(_pluginDirectoryObserver);

            // If the current plugin directory path value is not null/whitespace
            if (String.IsNullOrWhiteSpace(_optionsProvider.Current.PluginPath) is false)
            {
                // Try to prepopulate the channel with the current plugin directory value
                try
                {
                    // Write the current plugin path value to the channel
                    await _pluginPathChannel.Writer.WriteAsync(_optionsProvider.Current.PluginPath, stoppingToken).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.Log(LogLevel.Debug, exception, "Failed to set current plugin path");
                }
            }

            // Initialize the current path variable
            String? currentPath = null;

            // Idle until new plugin path values arrive or shutdown is requested
            while (await _pluginPathChannel.Reader.WaitToReadAsync(stoppingToken).ConfigureAwait(false))
            {
                // Initialize the updated path variable
                String? updatedPath = null;

                // Synchronously drain all available path values from the channel queue
                while (_pluginPathChannel.Reader.TryRead(out String? readValue))
                {
                    // If the path value is not null/whitespace
                    if (String.IsNullOrWhiteSpace(readValue) is false)
                    {
                        // Set the updated path to the read path
                        updatedPath = readValue;
                    }
                }

                // If the updated path value is the same as the current path value, continue with next loop
                if (updatedPath == currentPath) continue;

                // Set the current path value as the updated path value
                currentPath = updatedPath;

                // If the current path value is null/whitespace
                if (String.IsNullOrWhiteSpace(currentPath))
                {
                    if (_logger.IsEnabled(LogLevel.Warning))
                        _logger.Log(LogLevel.Warning, "Provided path value '{directory}' was null.", currentPath);

                    // Continue with next loop
                    continue;
                }

                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.Log(LogLevel.Information, "Plugin path value change detected");

                // Restart the module repository
                await RestartRepositoryAsync(stoppingToken).ConfigureAwait(false);
            }
        }

        private async Task RestartRepositoryAsync(CancellationToken cancellationToken = default)
        {
            // Unload all registered plugins
            await _pluginRepository.UnloadAllAsync(cancellationToken).ConfigureAwait(false);

            // Load the plugin repository's root directory
            await _pluginRepository.InitializeAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    internal sealed class PluginDirectoryObserver : IObserver<BotOptions>
    {
        /// <summary>
        /// The <see cref="ILogger"/> used to record operation logs.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// A <see cref="ChannelWriter{T}"/> for writing observed
        /// plugin directory path changes to a <see cref="Channel{T}"/>.
        /// </summary>
        private readonly ChannelWriter<String> _channelWriter;

        public PluginDirectoryObserver(ILogger logger, ChannelWriter<String> channelWriter)
        {
            _logger = logger;
            _channelWriter = channelWriter;
        }

        /// <inheritdoc/>
        void IObserver<BotOptions>.OnNext(BotOptions value)
        {
            // If the observed options has a null or empty token value
            if (String.IsNullOrWhiteSpace(value.PluginPath)) { return; }

            _logger.Log(LogLevel.Debug, "Plugin directory observer detected a configuration change.");

            if (_channelWriter.TryWrite(value.PluginPath) is false)
            {
                _logger.Log(LogLevel.Information, "Failed to write new plugin directory");
            }
        }

        /// <inheritdoc/>
        void IObserver<BotOptions>.OnError(Exception error)
        {
            _logger.Log(LogLevel.Warning, error, "Plugin directory observer received an error.");
        }

        /// <inheritdoc/>
        void IObserver<BotOptions>.OnCompleted()
        {
            _logger.Log(LogLevel.Information, "Plugin directory observer received a completion.");
        }
    }
}
