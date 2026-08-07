using Mauren.AspNetCore.Mvc;
using System.Reflection;

namespace Mauren.Discord.UI.Features
{
    internal static class StaticFiles
    {
        /// <summary>
        /// A reference to the site logo.
        /// </summary>
        internal static StaticFile Logo => new StaticFile
        {
            SourceAssembly = Assembly.GetExecutingAssembly(),
            Path = "~/img/Discord-Symbol-Blurple.png",
        };
    }
}
