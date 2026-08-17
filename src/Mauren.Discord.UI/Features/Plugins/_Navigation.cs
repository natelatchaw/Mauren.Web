using Mauren.AspNetCore.Mvc.Navigation;

namespace Mauren.Discord.UI.Features.Plugins
{
    internal static class Navigation
    {
        internal static IMenu? Menu => new NavigationMenu
        {
            Header = Manifest.Plugins.Header,
            Sections = new List<NavigationSection>
            {
                // General Section
                new NavigationSection
                {
                    Header = null,
                    Links = new List<NavigationLink>
                    {
                        Manifest.Plugins.Location,
                        Manifest.Plugins.Installed,
                        Manifest.Plugins.Upload,
                        Manifest.Plugins.Commands,
                    },
                    ShowDivider = true,
                }
            },
            ShowDivider = true,
        };
    }
}
