using Microsoft.Extensions.Configuration;
using System;

namespace Mauren.Discord.Application.Features.Configuration
{
    /// <summary>
    /// Strongly-typed application settings for the bot.
    /// </summary>
    public class BotOptions
    {
        /// <summary>
        /// The Discord bot token required for authentication.
        /// </summary>
        [ConfigurationKeyName("Token")]
        public String? Token { get; set; }

        /// <summary>
        /// A timestamp representing the last time the Discord Bot token was updated.
        /// </summary>
        [ConfigurationKeyName("TokenUpdated")]
        public DateTimeOffset TokenUpdated { get; set; }

        /// <summary>
        /// A <see cref="String"/> representing the path in which plugins are stored.
        /// </summary>
        public String? PluginPath { get; set; }
    }
}
