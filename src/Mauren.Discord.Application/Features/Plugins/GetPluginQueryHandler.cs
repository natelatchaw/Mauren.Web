using Mauren.Discord.Application.Abstractions.Messaging;
using Mauren.Discord.Application.Abstractions.Plugins;
using Mauren.Discord.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Application.Features.Plugins
{
    /// <inheritdoc/>
    public class GetPluginQueryHandler : IQueryHandler<GetPluginQuery, Result<PluginMetadata>>
    {
        private readonly IPluginRepository _pluginRepository;

        public GetPluginQueryHandler(IPluginRepository pluginRepository)
        {
            _pluginRepository = pluginRepository;
        }

        /// <inheritdoc/>
        async ValueTask<Result<PluginMetadata>> IQueryHandler<GetPluginQuery, Result<PluginMetadata>>.HandleAsync(GetPluginQuery query, CancellationToken cancellationToken)
        {
            try
            {
                // Get the metadata for the provided plugin identifier
                PluginMetadata pluginMetadata = await _pluginRepository.GetMetadata(query.PluginId, cancellationToken).ConfigureAwait(false);

                return Result<PluginMetadata>.Success(pluginMetadata);
            }
            catch (Exception exception)
            {
                return Result<PluginMetadata>.Failure(exception.Message);
            }
        }
    }
}
