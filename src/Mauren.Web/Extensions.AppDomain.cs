using System.IO;

namespace System
{
    public static class AppDomainExtensions
    {
        /// <summary>
        /// Gets the base directory that the assembly resolver uses to probe
        /// for assemblies, combined with the provided <paramref name="path"/>.
        /// </summary>
        /// 
        /// <param name="appDomain">
        /// An application domain.
        /// </param>
        /// 
        /// <param name="path">
        /// A <see cref="String"/> path to combine with the provided 
        /// <paramref name="appDomain"/>'s <see cref="AppDomain.BaseDirectory"/>.
        /// </param>
        /// 
        /// <returns>
        /// The base directory that the assembly resolver uses to probe
        /// for assemblies, combined with the provided <paramref name="path"/>.
        /// </returns>
        public static String BaseDirectoryWithPath(this AppDomain appDomain, String path)
        {
            // Combine the app domain's base directory with the provided path
            String combinedPath = Path.Combine(appDomain.BaseDirectory, path);
            // Initialize a new directory info instance from the combined path
            DirectoryInfo directory = new(combinedPath);
            // If the directory does not exist, create it
            if (directory.Exists is false) directory.Create();
            // Return the combined path
            return combinedPath;
        }
    }
}
