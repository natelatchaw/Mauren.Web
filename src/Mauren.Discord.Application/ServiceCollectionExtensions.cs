using Mauren.Discord.Application.Abstractions;
using Mauren.Discord.Application.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up Discord application services in an <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Discord application services and options configuration to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// 
        /// <param name="services">
        /// The <see cref="IServiceCollection"/> to add services to.
        /// </param>
        /// 
        /// <returns>
        /// The same <see cref="IServiceCollection"/> instance so that additional calls can be chained.
        /// </returns>
        public static IServiceCollection AddDiscordApplicationServices(this IServiceCollection services)
        {
            // Add the CQRS pipeline
            services.AddCQRSPipeline();

            return services;
        }

    }
}
