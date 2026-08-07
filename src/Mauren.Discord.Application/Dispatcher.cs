using Mauren.Discord.Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Application
{
    internal class Dispatcher : IDispatcher
    {
        /// <summary>
        /// The <see cref="ILogger{TCategoryName}"/> used to record operation logs.
        /// </summary>
        private readonly ILogger<Dispatcher> _logger;

        /// <summary>
        /// A <see cref="IServiceProvider"/> for resolving <see cref="ICommandHandler"/> instances.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        public Dispatcher(ILogger<Dispatcher> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        async ValueTask<Result> IDispatcher.DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken)
        {
            // Resolve the command handler service from the service provider
            ICommandHandler<TCommand, Result> handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand, Result>>();

            // Create a delegate for the command handler execution
            RequestHandlerDelegate<Result> handlerDelegate = () => handler.HandleAsync(command, cancellationToken);

            // Resolve all registered behaviors for the command type in registration order
            IEnumerable<IPipelineBehavior<TCommand, Result>> behaviors = _serviceProvider.GetServices<IPipelineBehavior<TCommand, Result>>();

            // Define the pipeline
            RequestHandlerDelegate<Result> pipeline = behaviors
                // Reverse the behavior collection
                .Reverse()
                // Aggregate the pipeline layer by layer
                .Aggregate(handlerDelegate, (next, behavior) => () => behavior.HandleAsync(command, next, cancellationToken));

            // Invoke the pipeline and return the result (always a ValueTuple)
            return await pipeline.Invoke();
        }

        /// <inheritdoc/>
        async ValueTask<Result<TResult>> IDispatcher.DispatchAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken)
        {
            // Resolve the query handler service from the service provider
            IQueryHandler<TQuery, Result<TResult>> handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, Result<TResult>>>();

            // Create a delegate for the query handler execution
            RequestHandlerDelegate<Result<TResult>> handlerDelegate = () => handler.HandleAsync(query, cancellationToken);

            // Resolve all registered behaviors for the query type in reverse registration order
            IEnumerable<IPipelineBehavior<TQuery, Result<TResult>>> behaviors = _serviceProvider.GetServices<IPipelineBehavior<TQuery, Result<TResult>>>();

            // Define the pipeline
            RequestHandlerDelegate<Result<TResult>> pipeline = behaviors
                // Reverse the behavior collection
                .Reverse()
                // Aggregate the pipeline layer by layer
                .Aggregate(handlerDelegate, (next, behavior) => () => behavior.HandleAsync(query, next, cancellationToken));

            // Invoke the pipeline and return the result
            return await pipeline.Invoke();
        }
    }
}
