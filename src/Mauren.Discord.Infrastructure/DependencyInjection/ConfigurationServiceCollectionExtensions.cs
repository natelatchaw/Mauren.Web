using Mauren.Discord.Application.Abstractions;
using Mauren.Discord.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up options provider services.
    /// </summary>
    internal static class ConfigurationServiceCollectionExtensions
    {
        /// <summary>
        /// Adds <see cref="IOptionsProvider{TOptions}"/> as a singleton service to the provided <paramref name="services"/>.
        /// Also registers the provided <paramref name="configuration"/> instance to bind <typeparamref name="TOptions"/> to,
        /// and adds the JSON configuration provider at the provided <paramref name="fileName"/>.
        /// </summary>
        /// 
        /// <typeparam name="TOptions">
        /// A <see langword="class"/> containing settings data to persist.
        /// </typeparam>
        /// 
        /// <param name="services">
        /// The <see cref="IServiceCollection"/> container to add <see cref="IOptionsProvider{TOptions}"/> to.
        /// </param>
        /// 
        /// <param name="configuration">
        /// The <see cref="IConfiguration"/> instance to bind <typeparamref name="TOptions"/> to.
        /// </param>
        /// 
        /// <param name="fileName">
        /// The name of the file to persist <typeparamref name="TOptions"/> to.
        /// </param>
        /// 
        /// <returns>
        /// The provided <paramref name="services"/> instance for chaining.
        /// </returns>
        internal static IServiceCollection AddOptionsProvider<TOptions>(this IServiceCollection services, IConfigurationManager configuration, String fileName)
            where TOptions : class
        {
            // Add the JSON configuration provider at the provided fileName
            configuration.TryAddJsonFile(fileName, optional: true, reloadOnChange: true);
            // Register the configuration instance to bind T to
            services.AddOptions<TOptions>().TryBind(configuration).ValidateOnStart();
            // Add the options provider service as a singleton
            services.TryAddSingleton<IOptionsProvider<TOptions>>(provider =>
            {
                ILogger<OptionsProvider<TOptions>> logger = provider.GetRequiredService<ILogger<OptionsProvider<TOptions>>>();
                IHostEnvironment environment = provider.GetRequiredService<IHostEnvironment>();
                IOptionsMonitor<TOptions> optionsMonitor = provider.GetRequiredService<IOptionsMonitor<TOptions>>();
                return new OptionsProvider<TOptions>(logger, environment, optionsMonitor, fileName);
            });
            // Return the service collection for chaining
            return services;
        }

        /// <summary>
        /// Trys to add the JSON configuration provider at 
        /// <paramref name="path"/> to <paramref name="builder"/> if it doesn't 
        /// already exist.
        /// </summary>
        /// 
        /// <param name="builder">
        /// The <see cref="IConfigurationBuilder"/> to add to.
        /// </param>
        /// 
        /// <param name="path">
        /// Path relative to the base path stored in 
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.
        /// </param>
        /// 
        /// <param name="optional">
        /// Whether the file is optional.
        /// </param>
        /// 
        /// <param name="reloadOnChange">
        /// Whether the configuration should be reloaded if the file changes.
        /// </param>
        /// 
        /// <returns>
        /// The <see cref="IConfigurationBuilder"/>.
        /// </returns>
        internal static IConfigurationBuilder TryAddJsonFile(this IConfigurationBuilder builder, String path, Boolean optional, Boolean reloadOnChange)
        {
            // Determine whether the provided path already exists
            Boolean exists = builder.Sources.OfType<JsonConfigurationSource>()
                .Where((JsonConfigurationSource source) => String.Equals(source.Path, path, StringComparison.OrdinalIgnoreCase))
                .Any();
            // If the source does not already exist, add it
            if (exists is false) builder.AddJsonFile(path, optional, reloadOnChange);
            // Return the options builder for chaining
            return builder;
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for adding configuration related options services to the DI
    /// container via <see cref="OptionsBuilder{TOptions}"/>.
    /// </summary>
    public static class OptionsBuilderConfigurationExtensions
    {
        /// <summary>
        /// Registers an <see cref="IConfiguration"/> instance which <typeparamref name="TOptions"/> will bind against, if it has not
        /// already been bound.
        /// </summary>
        /// 
        /// <typeparam name="TOptions">
        /// The options type to be configured.
        /// </typeparam>
        /// 
        /// <param name="optionsBuilder">
        /// The options builder to add the services to.
        /// </param>
        /// 
        /// <param name="configuration">
        /// The configuration being bound.
        /// </param>
        /// 
        /// <returns>
        /// The <see cref="OptionsBuilder{TOptions}"/> so that additional calls can be chained.
        /// </returns>
        internal static OptionsBuilder<TOptions> TryBind<TOptions>(this OptionsBuilder<TOptions> optionsBuilder, IConfiguration configuration)
            where TOptions : class
        {
            // Configure TOptions from configuration options
            ConfigureFromConfigurationOptions<TOptions> options = new(configuration);
            // Create a singleton service descriptor from the options
            ServiceDescriptor configureOptionsDescriptor = ServiceDescriptor.Singleton<IConfigureOptions<TOptions>>(options);
            // Add the service descriptor if it does not already exist
            optionsBuilder.Services.TryAddEnumerable(configureOptionsDescriptor);

            // Construct a change token source
            ConfigurationChangeTokenSource<TOptions> changeTokenSource = new(optionsBuilder.Name, configuration);
            // Create a singleton service descriptor from the change token source
            ServiceDescriptor changeTokenSourceDescriptor = ServiceDescriptor.Singleton<IOptionsChangeTokenSource<TOptions>>(changeTokenSource);
            // Add the service descriptor if it does not already exist
            optionsBuilder.Services.TryAddEnumerable(changeTokenSourceDescriptor);

            // Return the options builder
            return optionsBuilder;
        }
    }
}
