using Mauren.Discord.Application.Abstractions;
using Mauren.Discord.Application.Features.Configuration;
using Mauren.Discord.Infrastructure.Configuration;
using Mauren.Discord.Infrastructure.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Mauren.Discord.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up bot worker services.
    /// </summary>
    internal static class BotWorkerServiceCollectionExtensions
    {
        /// <summary>
        /// Adds
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        internal static IServiceCollection AddBotWorker(this IServiceCollection services)
        {
            // Add the bot worker controller as a singleton service
            services.AddSingleton<BotWorkerController<IBotWorker>>();
            // Add the bot worker controller interface to return the singleton concrete instance
            services.AddSingleton<IBotWorkerController<IBotWorker>>((IServiceProvider serviceProvider) =>
            {
                return serviceProvider.GetRequiredService<BotWorkerController<IBotWorker>>();
            });
            // Add the bot worker as a hosted service
            services.AddHostedService<BotWorker>();
            // Return the service collection for chaining
            return services;
        }
    }
}
