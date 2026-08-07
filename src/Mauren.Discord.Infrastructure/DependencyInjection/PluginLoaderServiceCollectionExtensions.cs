using Discord.Interactions;
using Mauren.Discord.Application.Abstractions;
using Mauren.Discord.Application.Abstractions.Modules;
using Mauren.Discord.Application.Abstractions.Plugins;
using Mauren.Discord.Infrastructure.Modules;
using Mauren.Discord.Infrastructure.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Mauren.Discord.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up plugin worker services.
    /// </summary>
    internal static class PluginWorkerServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the required infrastructure, repositories, and background services 
        /// to enable the dynamic plugin architecture.
        /// </summary>
        /// 
        /// <typeparam name="TContract">
        /// The base contract type for plugin discovery.
        /// </typeparam>
        /// 
        /// <param name="services">
        /// The service collection to add services to.
        /// </param>
        /// 
        /// <returns>
        /// The <see cref="IServiceCollection"/> for chaining.
        /// </returns>
        internal static IServiceCollection AddPluginWorker<TContract>(this IServiceCollection services)
        {
            // Add the plugin file provider factory as a singleton service
            services.TryAddSingleton<IPluginFileProviderFactory, PluginFileProviderFactory>();
            // Add the plugin archive manager as a singleton service
            services.TryAddSingleton<IPluginArchiveManager, PluginArchiveManager>();

            // Add the plugin repository service as a singleton service
            services.TryAddSingleton<PluginRepository<TContract>>();
            // Forward the non-generic plugin repository service interface to the generic plugin repository service interface
            services.TryAddSingleton<IPluginRepository>((IServiceProvider serviceProvider) =>
            {
                return serviceProvider.GetRequiredService<PluginRepository<TContract>>();
            });
            // Forward the generic plugin repository service interface to the generic plugin repository service interface
            services.TryAddSingleton<IPluginRepository<TContract>>((IServiceProvider serviceProvider) =>
            {
                return serviceProvider.GetRequiredService<PluginRepository<TContract>>();
            });

            // Add the module repository service as a singleton service
            services.TryAddSingleton<ModuleRepository<TContract>>();
            // Forward the non-generic module repository service interface to the module repository singleton service
            services.TryAddSingleton<IModuleRepository>((IServiceProvider serviceProvider) =>
            {
                return serviceProvider.GetRequiredService<ModuleRepository<TContract>>();
            });
            // Forward the generic module repository service interface to the module repository singleton service
            services.TryAddSingleton<IModuleRepository<TContract>>((IServiceProvider serviceProvider) =>
            {
                return serviceProvider.GetRequiredService<ModuleRepository<TContract>>();
            });

            // Forward the module registry service interface to the module repository singleton service
            services.TryAddSingleton<IModuleRegistry>((IServiceProvider serviceProvider) =>
            {
                return serviceProvider.GetRequiredService<ModuleRepository<TContract>>();
            });

            // Add the plugin worker as a hosted service
            services.AddHostedService<PluginWorker<TContract>>();

            // Return the service collection for chaining
            return services;
        }
    }
}
