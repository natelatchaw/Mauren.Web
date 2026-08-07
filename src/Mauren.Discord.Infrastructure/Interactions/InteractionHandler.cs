using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Mauren.Discord.Infrastructure.Modules;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Infrastructure.Interactions
{
    internal class InteractionHandler : BackgroundService
    {
        private readonly ILogger<InteractionHandler> _logger;
        private readonly DiscordSocketClient _socketClient;
        private readonly InteractionService _interactionService;
        private readonly IModuleRegistry _moduleRegistry;
        private readonly IInteractionCommandErrorHandler _commandErrorHandler;
        //private readonly IAdminService _adminService;
        //private readonly IOptionsMonitor<DebugOptions> _debugOptions;

        public InteractionHandler(ILogger<InteractionHandler> logger, DiscordSocketClient client,
            InteractionService interactionService, IModuleRegistry moduleRegistry,
            //IAdminService adminService, 
            //IOptionsMonitor<DebugOptions> debugOptions
            IInteractionCommandErrorHandler exceptionHandler 
            )
        {
            _logger = logger;
            _socketClient = client;
            _interactionService = interactionService;
            _moduleRegistry = moduleRegistry;
            //_adminService = adminService;
            //_debugOptions = debugOptions;
            _commandErrorHandler = exceptionHandler;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _socketClient.Ready += OnReady;
            _socketClient.SlashCommandExecuted += OnSlashCommandExecuted;
            _socketClient.ModalSubmitted += OnModalSubmitted;

            _interactionService.InteractionExecuted += OnInteractionExecuted;

            return Task.CompletedTask;
        }

        private async Task OnReady()
        {
            return;
#if DEBUG
            IEnumerable<UInt64>? guildIds = [];// _debugOptions.CurrentValue.GuildIds;
            if (guildIds is null)
            {
                // Ensure log level is enabled
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.Log(LogLevel.Warning, "Failed to register commands: no Guild IDs provided in configuration");
                guildIds ??= [];
            }
            foreach (UInt64 guildId in guildIds)
            {
                try
                {
                    IReadOnlyCollection<RestApplicationCommand> commands = await _interactionService
                        .RegisterCommandsToGuildAsync(guildId, deleteMissing: true);

                    foreach (RestApplicationCommand command in commands)
                        _logger.Log(LogLevel.Information, "Guild {guild} command registered: '{name}'", guildId, command.Name);
                }
                catch (Exception excpetion)
                {
                    // Ensure log level is enabled
                    if (_logger.IsEnabled(LogLevel.Warning))
                        _logger.Log(LogLevel.Warning, excpetion, "Guild {guild} command registration failed", guildId);
                }
            }
#else
            IReadOnlyCollection<RestApplicationCommand> commands = await _interactionService
                .RegisterCommandsGloballyAsync(deleteMissing: true);
#endif
        }

        private async Task OnSlashCommandExecuted(SocketSlashCommand socketSlashCommand)
        {
            SearchResult<SlashCommandInfo> searchResult = _interactionService.SearchSlashCommand(socketSlashCommand);
            if (searchResult.IsSuccess is false)
                throw new InvalidOperationException($"Lookup failed for {socketSlashCommand.CommandName}: Command not found");

            SlashCommandInfo slashCommandInfo = searchResult.Command;
            ModuleInfo moduleInfo = slashCommandInfo.Module;

            IServiceProvider serviceProvider = await _moduleRegistry.GetServiceProviderAsync(moduleInfo);

            SocketInteractionContext context = new(_socketClient, socketSlashCommand);
            IResult result = await _interactionService.ExecuteCommandAsync(context, serviceProvider);
        }

        private async Task OnModalSubmitted(SocketModal socketModal)
        {
            SearchResult<ModalCommandInfo> searchResult = _interactionService.SearchModalCommand(socketModal);
            if (searchResult.IsSuccess is false)
                throw new InvalidOperationException($"Lookup failed for {socketModal.Data.CustomId}: Command not found");

            ModalCommandInfo modalCommandInfo = searchResult.Command;
            ModuleInfo moduleInfo = modalCommandInfo.Module;

            IServiceProvider serviceProvider = await _moduleRegistry.GetServiceProviderAsync(moduleInfo);

            SocketInteractionContext context = new(_socketClient, socketModal);
            IResult result = await _interactionService.ExecuteCommandAsync(context, serviceProvider);
        }

        private async Task OnInteractionExecuted(ICommandInfo commandInfo, IInteractionContext interactionContext, IResult result)
        {
            Boolean hasResponded = interactionContext.Interaction.HasResponded;

            if (result.IsSuccess) return;

            // If the result specified a command error type
            if (result.Error is InteractionCommandError interactionCommandError)
            {
                // Try to notify the user
                try
                {
                    // Create an embed from the error type
                    Embed? embed = _commandErrorHandler.GetUserEmbed(commandInfo, interactionContext, result);
                    if (embed is null) throw new Exception();

                    // Return a task representing a follow-up message
                    Task userMessageTask = hasResponded switch
                    {
                        // The interaction has already been responded to
                        true => interactionContext.Interaction.FollowupAsync(embed: embed),
                        // The interaction has not already been responded to
                        false => interactionContext.Interaction.RespondAsync(embed: embed),
                    };
                    await userMessageTask;
                }
                catch (Exception exception) { }

                // Try to notify the owner
                try
                {
                    /*
                    // If admin messaging is disabled
                    if (_adminService.AreMessagesEnabled is false) return;

                    // Get the owner
                    IUser user = await _adminService.GetOwnerAsync();

                    // Create a DM channel with the owner
                    IDMChannel channel = await user.CreateDMChannelAsync();

                    // Create an embed from the error type
                    Embed? embed = _commandErrorHandler.GetOwnerEmbed(commandInfo, interactionContext, result);
                    if (embed is null) throw new Exception();

                    // Send the message
                    Task<IUserMessage> ownerMessageTask = channel.SendMessageAsync(embed: embed);
                    await ownerMessageTask;
                    */
                }
                catch (Exception exception) { }
            }
        }
    }

    public static class Extensions
    {
        public static SearchResult<ModalCommandInfo> SearchModalCommand(this InteractionService interactionService, SocketModal socketModal)
        {
            ModalCommandInfo? modalCommandInfo = interactionService.ModalCommands
                .Where((ModalCommandInfo m) => m.Name == socketModal.Data.CustomId)
                .SingleOrDefault();

            return modalCommandInfo switch
            {
                ModalCommandInfo value => SearchResult<ModalCommandInfo>.FromSuccess(String.Empty, value),
                _ => SearchResult<ModalCommandInfo>.FromError(new InvalidOperationException())
            };
        }
    }
}