using System.Threading.Tasks;

namespace Mauren.Discord.Application.Abstractions.Messaging
{
    /// <summary>
    /// Represents an asynchronous delegate that executes the next behavior in the pipeline, 
    /// or the final handler if no other behaviors remain.
    /// </summary>
    /// 
    /// <typeparam name="TResult">
    /// The expected return type produced by the pipeline.
    /// </typeparam>
    /// 
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> containing the result of the execution.
    /// </returns>
    public delegate ValueTask<TResult> RequestHandlerDelegate<TResult>();
}
