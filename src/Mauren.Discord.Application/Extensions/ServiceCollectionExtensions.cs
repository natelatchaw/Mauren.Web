using Mauren.Discord.Application.Abstractions.Messaging;
using Mauren.Discord.Application.Abstractions.Validation;
using Mauren.Discord.Application.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mauren.Discord.Application.Extensions
{
    /// <summary>
    /// Extension methods for setting up the CQRS pipeline in an <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Automatically discovers and registers all of the following in the provided assembly:
        /// <list type="bullet">
        /// <item><see cref="ICommandHandler{TCommand}"/> implementations</item>
        /// <item><see cref="IPipelineBehavior{TCommand}"/> implementations</item>
        /// <item><see cref="IQueryHandler{TQuery, TResult}"/> implementations</item>
        /// <item><see cref="IQueryBehavior{TQuery, TResult}"/> implementations</item>
        /// <item><see cref="IValidator{TCommand}"/> implementations</item>
        /// </list>
        /// </summary>
        public static IServiceCollection AddCQRSPipeline(this IServiceCollection services)
        {
            // Get the current assembly
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Add the command dispatcher as a singleton service
            services.AddSingleton<IDispatcher, Dispatcher>();

            // Register command behavior for validation
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            // Get all types in the assembly
            IEnumerable<Type> types = assembly.GetTypes()
                // Filter to non-abstract class types
                .Where((Type type) => type is { IsClass: true, IsAbstract: false });

            // Define the open generic types to scan and register
            List<Type> targetGenericTypeDefinitions = new List<Type>
            {
                typeof(ICommandHandler<,>),
                typeof(IQueryHandler<,>),
                typeof(IValidator<>),
            };

            // Iterate over the discovered types
            foreach (Type type in types)
            {
                // Iterate over interfaces implemented by the type
                foreach (Type @interface in type.GetInterfaces())
                {
                    // If the interface is generic, skip it
                    if (@interface.IsGenericType is false) continue;

                    // Get the interface's generic type definition
                    Type genericTypeDefinition = @interface.GetGenericTypeDefinition();

                    // If the type definition matches a target open generic type
                    if (targetGenericTypeDefinitions.Contains(genericTypeDefinition))
                    {
                        // Add the type as a transient service
                        services.AddTransient(@interface, type);
                    }
                }
            }

            // Return the service collection for chaining
            return services;
        }
    }
}