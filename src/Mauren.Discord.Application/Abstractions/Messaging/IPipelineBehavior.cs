using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Application.Abstractions.Messaging
{
    /// <summary>
    /// Represents a middleware behavior that wraps the execution of a request handler pipeline.
    /// </summary>
    /// 
    /// <typeparam name="TRequest">
    /// The type of request being processed.
    /// </typeparam>
    /// 
    /// <typeparam name="TResult">
    /// The expected return type produced by the <see cref="IPipelineBehavior{TRequest, TResult}"/>.
    /// </typeparam>
    internal interface IPipelineBehavior<in TRequest, TResult>
    {
        /// <summary>
        /// Asynchronously handles the <typeparamref name="TRequest"/>, optionally performing 
        /// pre- or post-processing operations before or after invoking the next delegate
        /// in the pipeline.
        /// </summary>
        /// 
        /// <param name="request">
        /// The request instance being handled.
        /// </param>
        /// 
        /// <param name="next">
        /// An asynchronous delegate representing the next behavior in the pipeline, or the 
        /// final request handler if no other behaviors remain.
        /// </param>
        /// 
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> used to observe cancellation requests.
        /// </param>
        /// 
        /// <returns>
        /// A <see cref="ValueTask"/> representing the asynchronous execution of the behavior pipeline,
        /// containing the <typeparamref name="TResult"/> of the query pipeline execution.
        /// </returns>
        ValueTask<TResult> HandleAsync(TRequest request, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken = default);
    }
}
