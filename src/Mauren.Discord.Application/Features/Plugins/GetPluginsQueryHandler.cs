using Mauren.Discord.Application.Abstractions.Messaging;
using Mauren.Discord.Application.Abstractions.Plugins;
using Mauren.Discord.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Application.Features.Plugins
{
    /// <inheritdoc/>
    public class GetPluginsQueryHandler : IQueryHandler<GetPluginsQuery, Result<IEnumerable<PluginMetadata>>>
    {
        private readonly IPluginRepository _pluginRepository;

        public GetPluginsQueryHandler(IPluginRepository pluginRepository)
        {
            _pluginRepository = pluginRepository;
        }

        /// <inheritdoc/>
        async ValueTask<Result<IEnumerable<PluginMetadata>>> IQueryHandler<GetPluginsQuery, Result<IEnumerable<PluginMetadata>>>.HandleAsync(GetPluginsQuery query, CancellationToken cancellationToken)
        {
            try
            {
                // Get the metadata for all stored plugins
                IEnumerable<PluginMetadata> result = await _pluginRepository.GetAllMetadata(cancellationToken).ConfigureAwait(false);

                return Result<IEnumerable<PluginMetadata>>.Success(result);
            }
            catch (Exception exception)
            {
                return Result<IEnumerable<PluginMetadata>>.Failure(exception.Message);
            }
        }
    }
}
