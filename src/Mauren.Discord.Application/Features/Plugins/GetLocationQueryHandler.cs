using Mauren.Discord.Application.Abstractions;
using Mauren.Discord.Application.Abstractions.Messaging;
using Mauren.Discord.Application.Features.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Application.Features.Plugins
{
    /// <inheritdoc/>
    public class GetLocationQueryHandler : IQueryHandler<GetLocationQuery, Result<LocationInformation?>>
    {
        /// <summary>
        /// The <see cref="IOptionsProvider{TOptions}"/> for the bot options.
        /// </summary>
        private readonly IOptionsProvider<BotOptions> _optionsProvider;

        /// <inheritdoc/>
        public GetLocationQueryHandler(IOptionsProvider<BotOptions> optionsProvider)
        {
            _optionsProvider = optionsProvider;
        }

        /// <inheritdoc/>
        ValueTask<Result<LocationInformation?>> IQueryHandler<GetLocationQuery, Result<LocationInformation?>>.HandleAsync(GetLocationQuery query, CancellationToken cancellationToken)
        {
            // Read the current plugin path
            String? path = _optionsProvider.Current.PluginPath;

            LocationInformation? locationInformation = new()
            {
                Value = path,
            };

            Result<LocationInformation?> result = Result<LocationInformation?>.Success(locationInformation);

            // Return the read token
            return ValueTask.FromResult(result);
        }
    }

    public class LocationInformation
    {
        /// <summary>
        /// The current path of the plugin directory.
        /// </summary>
        public String? Value { get; set; }
    }
}
