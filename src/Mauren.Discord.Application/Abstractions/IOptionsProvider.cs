using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Application.Abstractions
{
    /// <summary>
    /// Provides mechanisms to update and persist configuration options
    /// of <see langword="type"/> <typeparamref name="TOptions"/>.
    /// </summary>
    /// 
    /// <typeparam name="TOptions">
    /// The <see langword="type"/> of options being managed.
    /// </typeparam>
    public interface IOptionsProvider<TOptions> : IObservable<TOptions>
    {
        /// <summary>
        /// A thread-safe snapshot if the current options state.
        /// </summary>
        TOptions Current { get; }

        /// <summary>
        /// Asynchronously updates the <typeparamref name="TOptions"/> value via 
        /// <paramref name="configureOptions"/>.
        /// </summary>
        /// 
        /// <param name="configureOptions">
        /// An <see cref="Action{T}"/> with which to update the 
        /// <typeparamref name="TOptions"/> value.
        /// </param>
        /// 
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting 
        /// for the task to complete.
        /// </param>
        /// 
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous update 
        /// operation.
        /// </returns>
        Task UpdateAsync(Action<TOptions> configureOptions, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously saves changes to the underlying storage provider. 
        /// </summary>
        /// 
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting 
        /// for the task to complete.
        /// </param>
        /// 
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous save 
        /// operation.
        /// </returns>
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
