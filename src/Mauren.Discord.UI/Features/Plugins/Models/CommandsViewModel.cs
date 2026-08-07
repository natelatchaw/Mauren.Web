using Mauren.Discord.Application.Features.Plugins;

namespace Mauren.Discord.UI.Features.Plugins.Models
{
    internal class CommandsViewModel
    {
        public IEnumerable<GuildMetadataViewModel>? Guilds { get; set; }
    }
}
