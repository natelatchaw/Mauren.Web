using Mauren.Discord.Application.Abstractions;
using Mauren.Discord.Application.Features.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mauren.Discord.Infrastructure.Configuration
{
    /// <inheritdoc/>
    internal class BotWorkerController<TBotWorker> : IBotWorkerController<TBotWorker>
    {
        private readonly Channel<SetBotWorkerStatusCommand> _channel = Channel.CreateUnbounded<SetBotWorkerStatusCommand>();
        private volatile Boolean _isRunning = false;

        /// <inheritdoc/>
        Boolean IBotWorkerController<TBotWorker>.IsRunning => _isRunning;

        /// <inheritdoc/>
        async ValueTask<ValueTuple> IBotWorkerController<TBotWorker>.SendCommandAsync(SetBotWorkerStatusCommand command, CancellationToken cancellationToken)
        {
            await _channel.Writer.WriteAsync(command, cancellationToken);

            return ValueTuple.Create();
        }

        /// <summary>
        /// Submits a <see cref="SetBotWorkerStatusCommand.Resume"/> command to the channel.
        /// </summary>
        /// 
        /// <param name="cancellationToken">
        /// </param>
        /// 
        /// <returns>
        /// A <see cref="ValueTask"/> representing the asynchronous operation.
        /// </returns>
        internal async ValueTask StartAsync(CancellationToken cancellationToken)
        {
            await _channel.Writer.WriteAsync(SetBotWorkerStatusCommand.Resume, cancellationToken);
        }

        internal void Update(Boolean isRunning)
        {
            _isRunning = isRunning;
        }

        internal IAsyncEnumerable<SetBotWorkerStatusCommand> ReadAllAsync(CancellationToken cancellationToken)
        {
            return _channel.Reader.ReadAllAsync(cancellationToken);
        }
    }
}
