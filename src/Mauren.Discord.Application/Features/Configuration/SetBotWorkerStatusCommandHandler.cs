using Mauren.Discord.Application.Abstractions;
using Mauren.Discord.Application.Abstractions.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Application.Features.Configuration
{
    /// <summary>
    /// A marker interface for the bot worker service.
    /// </summary>
    public interface IBotWorker { }

    /// <inheritdoc/>
    public class SetBotWorkerStatusCommandHandler : ICommandHandler<SetBotWorkerStatusCommand, Result>
    {
        private readonly IBotWorkerController<IBotWorker> _controller;

        public SetBotWorkerStatusCommandHandler(IBotWorkerController<IBotWorker> controller)
        {
            _controller = controller;
        }

        /// <inheritdoc/>
        async ValueTask<Result> ICommandHandler<SetBotWorkerStatusCommand, Result>.HandleAsync(SetBotWorkerStatusCommand command, CancellationToken cancellationToken)
        {
            // Determine the expected state based on the command
            Boolean isRunning = command == SetBotWorkerStatusCommand.Resume;

            // If the expected state is already true
            if (_controller.IsRunning == isRunning) return Result.Success();

            // Send the command
            await _controller.SendCommandAsync(command, cancellationToken);

            // Create a cancellation token source and link the provided cancellation token
            using CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            // Enforce a maximum wait time, after which the operation is cancelled
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(3));

            try
            {
                // While the controller's reported state does not match the expected state
                while(_controller.IsRunning != isRunning)
                {
                    // Delay for 50ms
                    await Task.Delay(50, cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // The maximum wait time was reached or the provided cancellation token was cancelled.
                // Simply swallow the exception, the reported value when queried may be inaccurate until
                // the worker service manages to catch up.
            }
            
            // Return a created value tuple
            return Result.Success();
        }
    }
}
