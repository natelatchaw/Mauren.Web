using Mauren.Discord.Application.Abstractions.Messaging;
using Mauren.Discord.Application.Abstractions.Plugins;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Application.Features.Plugins
{
    /// <inheritdoc/>
    public class UploadPluginCommandHandler : ICommandHandler<UploadPluginCommand, Result>
    {
        private readonly IPluginRepository _pluginRepository;
        private readonly IPluginArchiveManager _pluginArchiveManager;

        public UploadPluginCommandHandler(IPluginRepository pluginRepository, IPluginArchiveManager pluginArchiveManager)
        {
            _pluginRepository = pluginRepository;
            _pluginArchiveManager = pluginArchiveManager;
        }

        /// <inheritdoc/>
        async ValueTask<Result> ICommandHandler<UploadPluginCommand, Result>.HandleAsync(UploadPluginCommand command, CancellationToken cancellationToken)
        {
            String? subpath = null;

            try
            {
                // Extract the zip stream via the plugin archive manager
                subpath = await _pluginArchiveManager.ExtractArchiveAsync(command.FileStream, cancellationToken).ConfigureAwait(false);

                // Load the extracted subpath via the plugin repository
                await _pluginRepository.LoadAsync(subpath, cancellationToken).ConfigureAwait(false);

                return Result.Success();
            }
            catch (Exception exception)
            {
                // If the subpath is not null/whitespace
                if (String.IsNullOrWhiteSpace(subpath) is false)
                {
                    // Try to cleanup the extracted zip stream
                    try
                    {
                        // Delete the extracted archive directory
                        await _pluginArchiveManager.DeleteArchiveAsync(subpath, cancellationToken);
                    }
                    catch
                    {
                        // Swallow exceptions
                    }
                }

                return Result.Failure(exception.Message);
            }
        }
    }
}
