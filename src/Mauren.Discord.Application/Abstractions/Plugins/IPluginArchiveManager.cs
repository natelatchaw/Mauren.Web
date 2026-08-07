using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Application.Abstractions.Plugins
{
    public interface IPluginArchiveManager
    {
        /// <summary>
        /// Extracts an uploaded zip stream into a specific subdirectory within the plugin directory.
        /// </summary>
        /// 
        /// <param name="stream">
        /// The raw stream of the uploaded archive.
        /// </param>
        /// 
        /// <param name="cancellationToken">
        /// A token to monitor for cancellation requests.
        /// </param>
        /// 
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous extraction operation, 
        /// containing the relative subpath of the newly created plugin directory.
        /// </returns>
        Task<String> ExtractArchiveAsync(Stream stream, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an extracted plugin's subdirectory from the plugin directory.
        /// </summary>
        /// 
        /// <param name="subpath">
        /// </param>
        /// 
        /// <param name="cancellationToken">
        /// A token to monitor for cancellation requests.
        /// </param>
        /// 
        /// <returns>
        /// </returns>
        Task DeleteArchiveAsync(String subpath, CancellationToken cancellationToken = default);
    }
}
