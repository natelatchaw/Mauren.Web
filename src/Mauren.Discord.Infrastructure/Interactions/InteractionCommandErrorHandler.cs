using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Mauren.Discord.Infrastructure.Interactions
{
    internal interface IInteractionCommandErrorHandler
    {
        /// <summary>
        /// Generate an <see cref="Embed"/> containing information about
        /// why an interaction failed for the application user.
        /// </summary>
        /// 
        /// <param name="commandInfo">
        /// Information about the command that failed.
        /// </param>
        /// 
        /// <param name="interactionContext">
        /// Information about the interaction context.
        /// </param>
        /// 
        /// <param name="result">
        /// The result of the interaction.
        /// </param>
        /// 
        /// <returns>
        /// An <see cref="Embed"/> to be sent to the application user.
        /// </returns>
        public Embed? GetUserEmbed(ICommandInfo commandInfo, IInteractionContext interactionContext, IResult result);

        /// <summary>
        /// Generate an <see cref="Embed"/> containing information about
        /// why an interaction failed for the application owner.
        /// </summary>
        /// 
        /// <param name="commandInfo">
        /// Information about the command that failed.
        /// </param>
        /// 
        /// <param name="interactionContext">
        /// Information about the interaction context.
        /// </param>
        /// 
        /// <param name="result">
        /// The result of the interaction.
        /// </param>
        /// 
        /// <returns>
        /// An <see cref="Embed"/> to be sent to the application owner.
        /// </returns>
        public Embed? GetOwnerEmbed(ICommandInfo commandInfo, IInteractionContext interactionContext, IResult result);
    }

    internal class InteractionCommandErrorHandler : IInteractionCommandErrorHandler
    {
        private readonly ILogger<InteractionCommandErrorHandler> _logger;
        private readonly DiscordSocketClient _client;
        private readonly IOptionsMonitor<InteractionCommandErrorHandlerOptions> _options;

        public InteractionCommandErrorHandler(ILogger<InteractionCommandErrorHandler> logger,
            DiscordSocketClient client, IOptionsMonitor<InteractionCommandErrorHandlerOptions> options)
        {
            _logger = logger;
            _client = client;
            _options = options;
        }

        Embed? IInteractionCommandErrorHandler.GetOwnerEmbed(ICommandInfo commandInfo, IInteractionContext interactionContext, IResult result)
        {
            // Create an embed builder
            EmbedBuilder builder = new();

            if (result.Error is not InteractionCommandError error) return null;

            StringBuilder descriptionBuilder = new();
            descriptionBuilder.AppendLine(result.ErrorReason);

            builder.WithTitle("Application Exception");
            builder.WithDescription(descriptionBuilder.ToString());
            builder.WithAuthor(interactionContext.Interaction.User);
            builder.WithTimestamp(interactionContext.Interaction.CreatedAt);

            builder.AddField((EmbedFieldBuilder fieldBuilder) =>
            {
                fieldBuilder.WithName("Command Name");
                fieldBuilder.WithValue(commandInfo.Name);
                fieldBuilder.WithIsInline(false);
            });

            builder.AddField((EmbedFieldBuilder fieldBuilder) =>
            {
                fieldBuilder.WithName("Module Name");
                fieldBuilder.WithValue(commandInfo.Module);
                fieldBuilder.WithIsInline(false);
            });

            return builder.Build();
        }

        Embed? IInteractionCommandErrorHandler.GetUserEmbed(ICommandInfo commandInfo, IInteractionContext interactionContext, IResult result)
        {
            // Create an embed builder
            EmbedBuilder builder = new();

            if (result.Error is not InteractionCommandError error) return null;

            if (_options.CurrentValue.Metadata.TryGetValue(error, out InteractionCommandErrorMetadata? metadata) is false)
            {
                // Ensure log level is enabled
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.Log(LogLevel.Debug, "No default message found for {interactionCommandError} in configuration, using generic message", error);

                StringBuilder descriptionBuilder = new();
                descriptionBuilder.Append("Something went wrong.");
                descriptionBuilder.Append("Check the logs for more information.");
                metadata = new()
                {
                    Title = "Application Exception",
                    Description = descriptionBuilder.ToString()
                };
            }

            if (metadata.Title is String title)
                builder.WithTitle(title);
            if (metadata.Description is String description)
                builder.WithDescription(description);
            if (metadata.ImageUrl is Uri imageUrl)
                builder.WithImageUrl(imageUrl.AbsoluteUri);

            return builder.Build();
        }
    }
}
