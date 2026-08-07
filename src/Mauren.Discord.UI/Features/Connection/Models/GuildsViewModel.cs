using Mauren.Discord.Application.Features.Connection;

namespace Mauren.Discord.UI.Features.Connection.Models
{
    internal class GuildsViewModel
    {
        public IEnumerable<GuildMetadataViewModel>? Guilds { get; init; }
    }
}
