using Discord.Interactions;
using Mauren.Discord.Application.Features.Configuration;
using Mauren.Discord.Infrastructure;
using Mauren.Discord.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up Discord core services in an <see cref="IServiceCollection"/>.
    /// </summary>
    public static class DiscordCoreServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Discord infrastructure services and options configuration to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// 
        /// <param name="services">
        /// The <see cref="IServiceCollection"/> to add services to.
        /// </param>
        /// 
        /// <param name="configuration">
        /// The <see cref="IConfigurationManager"/> used to register configuration sources and bind options.
        /// </param>    
        /// 
        /// <returns>
        /// The same <see cref="IServiceCollection"/> instance so that additional calls can be chained.
        /// </returns>
        public static IServiceCollection AddDiscordInfrastructureServices(this IServiceCollection services, IConfigurationManager configuration)
        {
            // Add the options provider service and add bot options
            services.AddOptionsProvider<BotOptions>(configuration, "Content/data.json");
            // Add the bot worker
            services.AddBotWorker();
            // Add the plugin worker
            services.AddPluginWorker<IInteractionModuleBase>();

            // Add Discord.Net services
            services.AddDiscordNet(configuration, new Discord.Net.DiscordNetConfig
            {
                // Configure the Discord Socket
                DiscordSocketConfig = new Discord.WebSocket.DiscordSocketConfig
                {

                },
                // Configure the Command Service
                CommandServiceConfig = new Discord.Commands.CommandServiceConfig
                {

                },
                // Configure the Interaction Service
                InteractionServiceConfig = new Discord.Interactions.InteractionServiceConfig
                {
                    DefaultRunMode = Discord.Interactions.RunMode.Async,
                },
            });
            // Add Logging Binder service
            services.AddHostedService<LoggingBinderService>();

            // Return the service collection
            return services;
        }
    }
}
