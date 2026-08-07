using Mauren.Discord.UI.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Reflection;

// Fix for Feature-based architecture: View location in non-conventional location
//[assembly: ProvideApplicationPartFactory(typeof(ConsolidatedAssemblyApplicationPartFactory))]

namespace Microsoft.Extensions.DependencyInjection
{
    public static class UIServiceExtensions
    {
        public static IServiceCollection AddDiscordUIServices(this IServiceCollection services)
        {
            // Get the executing assembly
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Add Controllers with Views support
            IMvcBuilder mvcBuilder = services.AddControllersWithViews((MvcOptions options) =>
            {
                options.Conventions.Add(new FeatureConvention());
            });

            // Configure the Razor View Engine
            services.Configure<RazorViewEngineOptions>((RazorViewEngineOptions options) =>
            {
                // Add Feature-based organization support
                options.ViewLocationExpanders.Add(new FeatureViewLocationExpander());
            });

            // Return the service collection instance for chaining
            return services;
        }

        public static IApplicationBuilder UseDiscordWebUI(this IApplicationBuilder application)
        {
            // Return the application builder for chaining
            return application;
        }
    }
}
