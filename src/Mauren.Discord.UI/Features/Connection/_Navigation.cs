using Mauren.AspNetCore.Mvc.Navigation;

namespace Mauren.Discord.UI.Features.Connection
{
    internal static class Navigation
    {
        internal static IMenu? Menu => new NavigationMenu
        {
            Header = Manifest.Connection.Header,
            Sections = new List<NavigationSection>
            {
                // General Section
                new NavigationSection
                {
                    Header = null,
                    Links = new List<NavigationLink>
                    {
                        Manifest.Connection.Status,
                        Manifest.Connection.Guilds,
                    },
                    ShowDivider = true,
                }
            },
            ShowDivider = true,
        };
    }
}
