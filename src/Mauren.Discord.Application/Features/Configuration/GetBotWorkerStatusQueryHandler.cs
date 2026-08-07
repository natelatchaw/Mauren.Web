using Mauren.Discord.Application.Abstractions;
using Mauren.Discord.Application.Abstractions.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Application.Features.Configuration
{
    /// <inheritdoc/>
    public class GetBotWorkerStatusQueryHandler : IQueryHandler<GetBotWorkerStatusQuery, Result<Boolean>>
    {
        private readonly IBotWorkerController<IBotWorker> _controller;

        public GetBotWorkerStatusQueryHandler(IBotWorkerController<IBotWorker> controller)
        {
            _controller = controller;
        }

        /// <inheritdoc/>
        ValueTask<Result<Boolean>> IQueryHandler<GetBotWorkerStatusQuery, Result<Boolean>>.HandleAsync(GetBotWorkerStatusQuery query, CancellationToken cancellationToken)
        {
            // Read the state from the abstraction
            Result<Boolean> result = Result<Boolean>.Success(_controller.IsRunning);

            return ValueTask.FromResult<Result<Boolean>>(result);
        }
    }
}
