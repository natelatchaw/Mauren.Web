using Mauren.Discord.Application.Abstractions;
using Mauren.Discord.Application.Abstractions.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Application.Features.Configuration
{
    /// <inheritdoc/>
    public class GetTokenQueryHandler : IQueryHandler<GetTokenQuery, Result<TokenInformation?>>
    {
        /// <summary>
        /// The <see cref="IOptionsProvider{TOptions}"/> for the bot options.
        /// </summary>
        private readonly IOptionsProvider<BotOptions> _optionsProvider;

        /// <inheritdoc/>
        public GetTokenQueryHandler(IOptionsProvider<BotOptions> optionsProvider)
        {
            _optionsProvider = optionsProvider;
        }

        /// <inheritdoc/>
        ValueTask<Result<TokenInformation?>> IQueryHandler<GetTokenQuery, Result<TokenInformation?>>.HandleAsync(GetTokenQuery query, CancellationToken cancellationToken)
        {
            // Read the current token
            String? token = _optionsProvider.Current.Token;
            // Read the timestamp of the last token update
            DateTimeOffset? lastUpdated = _optionsProvider.Current.TokenUpdated;

            TokenInformation? tokenInformation = new()
            {
                Value = token,
                LastUpdated = lastUpdated,
            };

            Result<TokenInformation?> result = Result<TokenInformation?>.Success(tokenInformation);

            // Return the read token
            return ValueTask.FromResult(result);
        }
    }

    public class TokenInformation
    {
        /// <summary>
        /// The current value of the token.
        /// </summary>
        public String? Value { get; set; }

        /// <summary>
        /// A timestamp representing the last time the token was updated.
        /// </summary>
        public DateTimeOffset? LastUpdated { get; set; }
    }
}
