using Mauren.Discord.Core;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Mauren.Discord.Application.Features.Plugins
{
    public class PluginMetadataViewModel
    {
        /// <summary>
        /// Gets the unique identifier of the plugin.
        /// </summary>
        public required String Id { get; init; }

        /// <summary>
        /// Gets the plugin's name.
        /// </summary>
        public required String Name { get; init; }

        /// <summary>
        /// Gets an image/icon URL representing the plugin (in data:image/png;base64 format).
        /// </summary>
        public String? Cover { get; init; }

        /// <summary>
        /// The plugin's version.
        /// </summary>
        public Version? Version { get; init; }
    }

    public static class PluginTileExtensions
    {
        public static async Task<PluginMetadataViewModel> AsViewModel(this PluginMetadata pluginMetadata)
        {
            String? base64String = pluginMetadata.Icon switch
            {
                IFileInfo icon => await GetBase64StringAsync(icon),
                _ => null,
            };
            String? imageSrc = base64String switch
            {
                String value => String.Join(',', "data:image/png;base64", value),
                _ => null,
            };

            PluginMetadataViewModel viewModel = new()
            {
                Id = pluginMetadata.Id,
                Name = pluginMetadata.Manifest?.Name ?? "Not Provided",
                Version = pluginMetadata.Manifest?.Version,
                Cover = imageSrc,
            };

            return viewModel;
        }

        static async Task<String?> GetBase64StringAsync(IFileInfo icon)
        {
            try
            {
                // Construct a new memory stream
                using MemoryStream memoryStream = new();

                // Create a stream from the provided icon file information
                await using Stream stream = icon.CreateReadStream();
                // Copy the icon's stream to the memory stream
                await stream.CopyToAsync(memoryStream);

                Byte[] bytes = memoryStream.ToArray();
                return Convert.ToBase64String(bytes);
            }
            catch
            {
                return null;
            }
        }
    }
}