using Microsoft.AspNetCore.Http;

namespace Mauren.Discord.UI.Features.Plugins.Models
{
    public class UploadViewModel
    {
        public String? PluginId { get; set; }
        public IFormFile? File { get; set; }
    }
}
