using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Application.Abstractions.Messaging
{
    /// <summary>
    /// Provides a mechanism to dispatch commands and queries to their corresponding handlers 
    /// without coupling the caller to the specific handler implementation.
    /// </summary>
    public interface IDispatcher
    {
        /// <summary>
        /// Asynchronously dispatches a command to its registered <see cref="ICommandHandler{TCommand}"/>.
        /// </summary>
        /// 
        /// <typeparam name="TCommand">
        /// The type of command being dispatched. Must be non-<see langword="null"/>able.
        /// </typeparam>
        /// 
        /// <param name="command">
        /// The <typeparamref name="TCommand"/> instance to dispatch.
        /// </param>
        /// 
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> used to observe cancellation requests while locating or executing the handler.
        /// </param>
        /// 
        /// <returns>
        /// A <see cref="ValueTask"/> representing the completion of the dispatch and execution pipeline.
        /// </returns>
        ValueTask<Result> DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : notnull;

        /// <summary>
        /// Asynchronously dispatches a query to its registered <see cref="IQueryHandler{TQuery, TResult}"/> 
        /// and returns the computed <typeparamref name="TResult"/>.
        /// </summary>
        /// 
        /// <typeparam name="TQuery">
        /// The type of query being dispatched. Must be non-<see langword="null"/>able.
        /// </typeparam>
        /// 
        /// <typeparam name="TResult">
        /// The expected return type produced by the query handler.
        /// </typeparam>
        /// 
        /// <param name="query">
        /// The <typeparamref name="TQuery"/> instance to dispatch.
        /// </param>
        /// 
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> used to observe cancellation requests while locating or executing the handler.
        /// </param>
        /// 
        /// <returns>
        /// A <see cref="ValueTask{TResult}"/> containing the <typeparamref name="TResult"/> of the query execution.
        /// </returns>
        ValueTask<Result<TResult>> DispatchAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
            where TQuery : notnull;
    }
}
