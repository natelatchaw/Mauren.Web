using Microsoft.Extensions.FileProviders;
using System;

namespace Mauren.Discord.Application.Abstractions
{
    /// <summary>
    /// Provides access to the file system scoped specifically to the plugin directory.
    /// </summary>
    /// 
    /// <remarks>
    /// Utilizing a factory pattern allows the underlying <see cref="IFileProvider"/> to be recreated dynamically 
    /// if the plugin directory configuration changes at runtime.
    /// </remarks>
    public interface IPluginFileProviderFactory
    {
        /// <summary>
        /// Gets the configured file provider scoped to the current plugin directory.
        /// </summary>
        /// 
        /// <returns>
        /// An <see cref="IFileProvider"/> instance pointing to the plugin directory.
        /// </returns>
        /// 
        /// <exception cref="InvalidOperationException">
        /// Thrown when the plugin directory path has not been configured.
        /// </exception>
        IFileProvider GetProvider();
    }
}
