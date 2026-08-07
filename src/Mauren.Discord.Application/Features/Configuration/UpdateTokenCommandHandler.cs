using Mauren.Discord.Application.Abstractions;
using Mauren.Discord.Application.Abstractions.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Application.Features.Configuration
{
    /// <inheritdoc/>
    public class UpdateTokenCommandHandler : ICommandHandler<UpdateTokenCommand, Result>
    {
        /// <summary>
        /// The <see cref="IOptionsProvider{TOptions}"/> for the bot options.
        /// </summary>
        private readonly IOptionsProvider<BotOptions> _optionsProvider;

        /// <inheritdoc/>
        public UpdateTokenCommandHandler(IOptionsProvider<BotOptions> optionsProvider)
        {
            _optionsProvider = optionsProvider;
        }

        /// <inheritdoc/>
        public async ValueTask<Result> HandleAsync(UpdateTokenCommand command, CancellationToken cancellationToken = default)
        {
            // Update the token via the options provider
            await _optionsProvider.UpdateAsync((BotOptions options) =>
            {
                // Set the token value to the command's new token value
                options.Token = command.NewToken;
                // Set the token updated timestamp to now
                options.TokenUpdated = DateTimeOffset.UtcNow;
            }, cancellationToken);

            // Return a success result
            return Result.Success();
        }
    }
}
