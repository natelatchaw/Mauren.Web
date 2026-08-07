using Mauren.Discord.Application.Abstractions.Messaging;
using Mauren.Discord.Application.Abstractions.Modules;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Application.Features.Plugins
{
    public class SyncPluginsCommandHandler : ICommandHandler<SyncPluginsCommand, Result>
    {
        private readonly IModuleRepository _moduleRepository;

        public SyncPluginsCommandHandler(IModuleRepository moduleRepository)
        {
            _moduleRepository = moduleRepository;
        }

        /// <inheritdoc/>
        async ValueTask<Result> ICommandHandler<SyncPluginsCommand, Result>.HandleAsync(SyncPluginsCommand command, CancellationToken cancellationToken)
        {
            try
            {
                await _moduleRepository.SyncCommandsAsync(command.GuildId, cancellationToken).ConfigureAwait(false);

                return Result.Success();
            }
            catch (Exception exception)
            {
                return Result.Failure(exception.Message);
            }
        }
    }
}
