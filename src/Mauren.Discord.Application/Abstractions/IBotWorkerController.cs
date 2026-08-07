using Mauren.Discord.Application.Features.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Application.Abstractions
{
    public interface IBotWorkerController<TBackgroundService>
    {
        Boolean IsRunning { get; }

        ValueTask<ValueTuple> SendCommandAsync(SetBotWorkerStatusCommand command, CancellationToken cancellationToken = default);
    }
}
