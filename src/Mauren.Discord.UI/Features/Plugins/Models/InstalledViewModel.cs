using Mauren.Discord.Application.Features.Plugins;

namespace Mauren.Discord.UI.Features.Plugins.Models
{
    public class InstalledViewModel
    {
        public IEnumerable<PluginMetadataViewModel>? Plugins { get; set; }
    }
}
