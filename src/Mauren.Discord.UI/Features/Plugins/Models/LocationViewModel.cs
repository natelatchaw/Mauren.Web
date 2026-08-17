using System.ComponentModel;

namespace Mauren.Discord.UI.Features.Plugins.Models
{
    internal class LocationViewModel
    {
        [DisplayName("Current Path")]
        public String? CurrentPath { get; set; }

        [DisplayName("New Path")]
        public String? NewPath { get; set; }

        [DisplayName("Last Updated")]
        public DateTimeOffset? LastUpdated { get; set; }
    }
}
