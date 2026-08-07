using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mauren.Discord.Application.Features.Plugins
{
    public class GuildMetadataViewModel
    {
        /// <summary>
        /// Gets the unique identifier of the guild.
        /// </summary>
        public required UInt64 Id { get; init; }

        /// <summary>
        /// Gets the guild's name.
        /// </summary>
        public String? Name { get; init; }

        /// <summary>
        /// Gets an image/icon representing the guild.
        /// </summary>
        public String? IconUrl { get; init; }

        /// <summary>
        /// Gets the member count of the guild.
        /// </summary>
        public Int32 MemberCount { get; init; }
    }

    public static class GuildMetadataViewModelExtensions
    {
        public static async Task<GuildMetadataViewModel> AsViewModel(this IGuild guild)
        {
            IReadOnlyCollection<IGuildUser> users = await guild.GetUsersAsync();
            IEnumerable<IGuildUser> members = users.Where((IGuildUser user) => user.IsBot is false);
            GuildMetadataViewModel result = new GuildMetadataViewModel
            {
                Id = guild.Id,
                Name = guild.Name,
                IconUrl = guild.IconUrl,
                MemberCount = members.Count(),
            };
            return result;
        }
    }
}
