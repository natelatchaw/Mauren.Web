using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Application.Abstractions.Messaging
{
    /// <summary>
    /// Defines a contract for handling a specific type of application command.
    /// </summary>
    /// 
    /// <typeparam name="TCommand">
    /// The type of command to be processed. This type parameter is contravariant.
    /// </typeparam>
    internal interface ICommandHandler<in TCommand, TResult> where TResult : Result
    {
        /// <summary>
        /// Asynchronously processes the specified <paramref name="command"/>.
        /// </summary>
        /// 
        /// <param name="command">
        /// The command instance containing the parameters or payload required for execution.
        /// </param>
        /// 
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> used to observe cancellation requests while locating
        /// or executing the handler.
        /// </param>
        /// 
        /// <returns>
        /// A <see cref="ValueTask{TResult}"/> representing the completion of the dispatch and execution pipeline.
        /// </returns>
        ValueTask<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
    }
}
