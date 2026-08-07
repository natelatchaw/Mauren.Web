using System;

namespace Mauren.Discord.Application.Features.Configuration
{
    /// <summary>
    /// Represents a command to update the authentication token used by the Discord bot.
    /// </summary>
    /// 
    /// <param name="NewToken">
    /// The new Discord bot token to be applied.
    /// </param>
    public record UpdateTokenCommand(String NewToken);
}
