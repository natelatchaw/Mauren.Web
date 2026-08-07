using Mauren.Discord.Infrastructure.Interactions;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up interaction handler services.
    /// </summary>
    internal static class InteractionHandlerServiceCollectionExtensions
    {
        public static IServiceCollection AddInteractionHandler(this IServiceCollection services, IConfigurationManager configuration)
        {
            services.Configure<InteractionCommandErrorHandlerOptions>(configuration.GetSection("InteractionCommandErrorHandler"));
            services.AddTransient<IInteractionCommandErrorHandler, InteractionCommandErrorHandler>();
            services.AddHostedService<InteractionHandler>();

            return services;
        }
    }
}
