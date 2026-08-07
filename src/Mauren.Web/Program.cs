using Discord.Interactions;
using Mauren.Extensions.Plugins.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace Mauren.Web
{
    public class Program
    {
        public static async Task Main(String[] args)
        {
            // Create an application builder
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Enable static web assets
            builder.WebHost.UseStaticWebAssets();

            // Configure the application's services
            builder.Services.ConfigureServices(builder.Configuration);
            // Build the application host
            using WebApplication host = builder.Build();

            // Configure the application's request processing pipeline
            host.Configure(builder.Environment);

            // Run the application host
            await host.RunAsync();
        }
    }

    public static class ProgramExtensions
    {
        /// <summary>
        /// Register application services in the provided <paramref name="services"/>.
        /// </summary>
        /// 
        /// <param name="services">
        /// The <see cref="IServiceCollection"/> to register services in.
        /// </param>
        /// 
        /// <param name="configuration">
        /// The application's <see cref="IConfiguration"/> provider.
        /// </param>
        public static void ConfigureServices(this IServiceCollection services, IConfigurationManager configuration)
        {
            // Add appsettings in Content folder
            configuration.AddJsonFile("Content/appsettings.json", optional: true, reloadOnChange: true);

            // Add Discord application services
            services.AddDiscordApplicationServices();
            // Add Discord infrastructure services
            services.AddDiscordInfrastructureServices(configuration);
            // Add Discord UI services
            services.AddDiscordUIServices();

            // Add routing configuration
            services.Configure<Routing>(configuration.GetSection(nameof(Routing)));

            // Add Plugin loader
            services.AddPluginLoader<IInteractionModuleBase>(configuration, (PluginLoaderOptions options) =>
            {
                options.HostServiceDescriptors = services;
            });
            // Add Plugin hosted service manager
            services.AddHostedServiceManager<IInteractionModuleBase>();
        }

        /// <summary>
        /// Configure the application request processing pipeline.
        /// </summary>
        /// 
        /// <param name="application">
        /// The application host.
        /// </param>
        /// 
        /// <param name="environment">
        /// The application's <see cref="IWebHostEnvironment"/>.
        /// </param>
        public static void Configure(this IApplicationBuilder application, IWebHostEnvironment environment)
        {
            // If the current hosting environment is a development environment
            if (environment.IsDevelopment())
            {
                // Use the developer exception page
                application.UseDeveloperExceptionPage();
            }
            // Otherwise
            else
            {
                // Use the exception handler
                application.UseExceptionHandler("/Error");
                // Use HSTS
                application.UseHsts();
            }

            // Use HTTPS Redirection
            application.UseHttpsRedirection();

            // Use Routing
            application.UseRouting();

            // Use Authorization
            application.UseAuthorization();

            // Map Razor Pages
            application.UseEndpoints((IEndpointRouteBuilder builder) =>
            {
                // Map static assets
                builder.MapStaticAssets();
                
                // Map controllers
                builder.MapControllers();
            });
        }
    }
}
