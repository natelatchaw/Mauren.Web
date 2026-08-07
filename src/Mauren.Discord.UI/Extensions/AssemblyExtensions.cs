using System.Reflection;

namespace Microsoft.AspNetCore.Mvc
{
    internal static class AssemblyExtensions
    {
        /// <summary>
        /// Generates the RCL static content path (~/_content/{AssemblyName}/...) for a given file.
        /// </summary>
        /// 
        /// <param name="assembly">
        /// The assembly containing the static files.
        /// </param>
        /// <param name="relativePath">
        /// The path to the file inside wwwroot (e.g., "lib/...").
        /// </param>
        /// 
        /// <returns>
        /// The fully formatted virtual path for the Razor engine.
        /// </returns>
        public static String GetStaticContentPath(this Assembly assembly, String relativePath)
        {
            // Get the name of the provided assembly
            String assemblyName = assembly.GetName().Name switch
            {
                String value => value,
                _ => throw new InvalidOperationException($"Could not determine the name of assembly '{assembly.FullName}'"),
            };

            // Sanitize the provided relative path
            String sanitizedPath = relativePath.TrimStart('~', '/');

            // Assembly the path
            return String.Join('/', "~", "_content", assemblyName, sanitizedPath);
        }
    }
}
