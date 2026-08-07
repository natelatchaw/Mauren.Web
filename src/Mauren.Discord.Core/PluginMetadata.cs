using Microsoft.Extensions.FileProviders;
using System;
using System.IO;

namespace Mauren.Discord.Core
{
    public class PluginMetadata
    {
        public required String Id { get; set; }
        public required String Subpath { get; set; }
        public PluginManifest? Manifest { get; set; }
        public IFileInfo? Icon { get; set; }        
    }

    public class PluginManifest
    {
        public String? Name { get; set; }
        public String? Description { get; set; }
        public Version? Version { get; set; }
    }
}
