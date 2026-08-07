using Discord;
using Mauren.Discord.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Application.Features.Plugins
{
    /// <inheritdoc/>
    public class GetGuildsQueryHandler : IQueryHandler<GetGuildsQuery, Result<IEnumerable<IGuild>>>
    {
        private readonly IDiscordClient _client;

        public GetGuildsQueryHandler(IDiscordClient client)
        {
            _client = client;
        }

        /// <inheritdoc/>
        async ValueTask<Result<IEnumerable<IGuild>>> IQueryHandler<GetGuildsQuery, Result<IEnumerable<IGuild>>>.HandleAsync(GetGuildsQuery query, CancellationToken cancellationToken)
        {
            try
            {
                IReadOnlyCollection<IGuild> results = await _client.GetGuildsAsync(CacheMode.AllowDownload);

                return Result<IEnumerable<IGuild>>.Success(results);
            }
            catch (Exception exception)
            {
                return Result<IEnumerable<IGuild>>.Failure(exception.Message);
            }
        }
    }
}
