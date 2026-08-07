using Mauren.Discord.Application.Abstractions.Messaging;
using Mauren.Discord.Application.Abstractions.Plugins;
using Mauren.Discord.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Application.Features.Plugins
{
    public class RemovePluginCommandHandler : ICommandHandler<RemovePluginCommand, Result>
    {
        private readonly IPluginRepository _pluginRepository;
        private readonly IPluginArchiveManager _pluginArchiveManager;

        public RemovePluginCommandHandler(IPluginRepository pluginRepository, IPluginArchiveManager pluginArchiveManager)
        {
            _pluginRepository = pluginRepository;
            _pluginArchiveManager = pluginArchiveManager;
        }

        /// <inheritdoc/>
        async ValueTask<Result> ICommandHandler<RemovePluginCommand, Result>.HandleAsync(RemovePluginCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // Get the metadata for the provided plugin identifier
                PluginMetadata pluginMetadata = await _pluginRepository.GetMetadata(command.PluginId, cancellationToken).ConfigureAwait(false);

                // If the metadata's subpath is invalid
                if (String.IsNullOrWhiteSpace(pluginMetadata.Subpath))
                    throw new InvalidOperationException($"Could not determine subpath for plugin '{pluginMetadata.Id}'");

                // Unload the plugin via the metadata's subpath
                await _pluginRepository.UnloadAsync(pluginMetadata.Subpath, cancellationToken).ConfigureAwait(false);

                // Delete the plugin via the metadata's subpath
                await _pluginArchiveManager.DeleteArchiveAsync(pluginMetadata.Subpath, cancellationToken).ConfigureAwait(false);

                return Result.Success();
            }
            catch (Exception exception)
            {
                return Result.Failure(exception.Message);
            }
        }
    }
}
