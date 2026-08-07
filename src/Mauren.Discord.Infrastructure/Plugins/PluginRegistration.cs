using Microsoft.Extensions.FileProviders;
using System;

namespace Mauren.Discord.Infrastructure.Plugins
{
    internal class PluginRegistration
    {
        public required String Id { get; set; }
        public required String Subpath { get; set; }
        public required IFileInfo FileInfo { get; set; }
    }

}
