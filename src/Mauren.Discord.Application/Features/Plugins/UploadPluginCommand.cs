using System;
using System.IO;

namespace Mauren.Discord.Application.Features.Plugins
{
    public record UploadPluginCommand(String FileName, String ContentType, Stream FileStream);
}
