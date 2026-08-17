using System;

namespace Mauren.Discord.Application.Features.Plugins
{
    /// <summary>
    /// Represents a command to update the location of the plugin directory.
    /// </summary>
    /// 
    /// <param name="NewLocation">
    /// The new location of the plugin directory.
    /// </param>
    public record UpdateLocationCommand(String NewLocation);
}
