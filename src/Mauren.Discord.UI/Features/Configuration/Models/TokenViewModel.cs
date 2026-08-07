using System.ComponentModel;

namespace Mauren.Discord.UI.Features.Configuration.Models
{
    internal class TokenViewModel
    {
        [DisplayName("Current Token")]
        public String? CurrentToken { get; set; }

        [DisplayName("New Token")]
        public String? NewToken { get; set; }

        [DisplayName("Last Updated")]
        public DateTimeOffset? LastUpdated { get; set; }
    }
}
