using Mauren.Discord.Application.Abstractions;
using Mauren.Discord.Application.Abstractions.Messaging;
using Mauren.Discord.Application.Features.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Application.Features.Plugins
{
    /// <inheritdoc/>
    public class UpdateLocationCommandHandler : ICommandHandler<UpdateLocationCommand, Result>
    {
        /// <summary>
        /// The <see cref="IOptionsProvider{TOptions}"/> for the bot options.
        /// </summary>
        private readonly IOptionsProvider<BotOptions> _optionsProvider;

        /// <inheritdoc/>
        public UpdateLocationCommandHandler(IOptionsProvider<BotOptions> optionsProvider)
        {
            _optionsProvider = optionsProvider;
        }

        /// <inheritdoc/>
        public async ValueTask<Result> HandleAsync(UpdateLocationCommand command, CancellationToken cancellationToken = default)
        {
            // Update the token via the options provider
            await _optionsProvider.UpdateAsync((BotOptions options) =>
            {
                // Set the token value to the command's new token value
                options.PluginPath = command.NewLocation;
            }, cancellationToken);

            // Return a success result
            return Result.Success();
        }
    }
}
