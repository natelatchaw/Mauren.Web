using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Application.Abstractions.Messaging
{
    /// <summary>
    /// Defines a contract for handling a specific type of application query.
    /// </summary>
    /// 
    /// <typeparam name="TQuery">
    /// The type of query to be processed. This type parameter is contravariant.
    /// </typeparam>
    /// 
    /// <typeparam name="TResult">
    /// The type of result produced by the <typeparamref name="TQuery"/>.
    /// </typeparam>
    public interface IQueryHandler<in TQuery, TResult> where TResult : Result
    {
        /// <summary>
        /// Asynchronously processes the specified <paramref name="query"/>.
        /// </summary>
        /// 
        /// <param name="query">
        /// The query instance containing the parameters or payload required for execution.
        /// </param>
        /// 
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> used to observe cancellation requests during execution.
        /// </param>
        /// 
        /// <returns>
        /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation and containing the result.
        /// </returns>
        ValueTask<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
    }
}
