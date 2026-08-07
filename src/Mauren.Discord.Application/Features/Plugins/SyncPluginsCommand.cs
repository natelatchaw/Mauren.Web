using System;

namespace Mauren.Discord.Application.Features.Plugins
{
    public record SyncPluginsCommand(UInt64? GuildId = default);
}
