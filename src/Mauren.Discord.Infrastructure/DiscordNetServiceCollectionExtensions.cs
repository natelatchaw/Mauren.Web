using Discord.Net;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up <see cref="Discord.Net"/> services in an <see cref="IServiceCollection"/>.
    /// </summary>
    internal static class DiscordNetServiceCollectionExtensions
    {
        public static IServiceCollection AddDiscordNet(this IServiceCollection services, IConfigurationManager configuration, DiscordNetConfig discordNetConfig)
        {
            // Add the socket client as a singleton service
            services.AddSingleton<Discord.WebSocket.DiscordSocketClient>((IServiceProvider serviceProvider) =>
            {
                // Get the provided Discord socket config instance (or create a new instance)
                Discord.WebSocket.DiscordSocketConfig config = discordNetConfig.DiscordSocketConfig ?? new();
                // Return the constructed socket client
                return new Discord.WebSocket.DiscordSocketClient(config);
            });
            // Bind the IDiscordClient interface to the singleton implementation service
            services.AddSingleton<Discord.IDiscordClient>((IServiceProvider serviceProvider) =>
            {
                return serviceProvider.GetRequiredService<Discord.WebSocket.DiscordSocketClient>();
            });

            // Add the command service as a singleton service
            services.AddSingleton<Discord.Commands.CommandService>((IServiceProvider serviceProvider) =>
            {
                // Get the provided Discord socket config instance (or create a new instance)
                Discord.Commands.CommandServiceConfig config = discordNetConfig.CommandServiceConfig ?? new();
                // Return the constructed socket client
                return new Discord.Commands.CommandService(config);
            });

            // Add the interaction service as a singleton service
            services.AddSingleton<Discord.Interactions.InteractionService>((IServiceProvider serviceProvider) =>
            {
                // Get the rest client provider from the service provider
                Discord.Rest.IRestClientProvider restClientProvider = serviceProvider.GetRequiredService<Discord.WebSocket.DiscordSocketClient>();
                // Get the provided interaction service config instance (or create a new instance)
                Discord.Interactions.InteractionServiceConfig config = discordNetConfig.InteractionServiceConfig ?? new();
                // Return the constructed interaction service
                return new Discord.Interactions.InteractionService(restClientProvider, config);
            });

            // Add interaction handler services
            services.AddInteractionHandler(configuration);

            return services;
        }
    }
}