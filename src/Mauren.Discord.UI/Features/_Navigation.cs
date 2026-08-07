using Bootstrap.Interfaces;
using Mauren.AspNetCore.Mvc;
using Mauren.AspNetCore.Mvc.Navigation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Mauren.Discord.UI.Features
{
    internal static class Navigation
    {
        internal static IBar Bar => new NavigationBar
        {
            Logo = StaticFiles.Logo,
            LogoLink = Manifest.Header,
            //Links = new List<INavigationLink>
            //{
            //    Manifest.Configuration.Nav,
            //    Manifest.Connection.Nav,
            //    Manifest.Plugins.Nav,
            //},
            Dropdowns = new Dictionary<ILabelable, IEnumerable<INavigationLink>>
            {
                // Configuration
                {
                    Manifest.Configuration.Header, 
                    new List<INavigationLink>
                    {
                        Manifest.Configuration.Status,
                        Manifest.Configuration.Token,
                    } 
                },
                // Connection
                {
                    Manifest.Connection.Header,
                    new List<INavigationLink>
                    {
                        Manifest.Connection.Status,
                        Manifest.Connection.Guilds,
                    }
                },
                // Plugins
                {
                    Manifest.Plugins.Header,
                    new List<INavigationLink>
                    {
                        Manifest.Plugins.Installed,
                        Manifest.Plugins.Upload,
                        Manifest.Plugins.Commands,
                    }
                }
            },
            ShowToggler = true,
        };
    }

    internal class Icon : IHasIcon
    {
        public required String IconName { get; init; }
    }

    internal class NavigationBar : Mauren.AspNetCore.Mvc.Navigation.IBar
    {
        public StaticFile? Logo { get; set; }
        public ILink? LogoLink { get; set; }
        public IDictionary<ILabelable, IEnumerable<INavigationLink>>? Dropdowns { get; set; }
        public IEnumerable<INavigationLink>? Links { get; set; }
        public IEnumerable<String>? Partials { get; set; }
        public Boolean? ShowToggler { get; set; }
    }

    internal class NavigationMenu : Mauren.AspNetCore.Mvc.Navigation.IMenu
    {
        public Guid Id { get; } = Guid.NewGuid();
        public INavigationLink? Header { get; init; }
        public IEnumerable<ISection>? Sections { get; init; }
        public Boolean ShowDivider { get; set; } = true;
    }

    internal class NavigationLink : INavigationLink
    {
        public required String Identifier { get; set; }
        public String? Label { get; set; }
        public String? IconName { get; set; }
        public String? Title { get; set; }
        public Bootstrap.LinkColor? LinkColor { get; set; }
        public Bootstrap.ButtonColor? ButtonColor { get; set; }
        public Bootstrap.ButtonStyle? ButtonStyle { get; set; }
        public IEnumerable<KeyValuePair<String, String?>> Properties { get; set; }
        public String? Area { get; set; }
        public String? Page { get; set; }
        public String? Handler { get; set; }
        public String? Controller { get; set; }
        public String? Action { get; set; }
        public Dictionary<String, String>? RouteValues { get; }

        Boolean IActivatable.IsActive(ViewContext viewContext)
        {
            if (viewContext.ViewData["ActivePage"] is String activePage)
                return String.Equals(activePage, Identifier, StringComparison.OrdinalIgnoreCase);

            else if (Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName) is String filename)
                return String.Equals(filename, Identifier, StringComparison.OrdinalIgnoreCase);

            else
                return false;
        }

        Boolean IHidable.IsHidden(ViewContext viewContext)
        {
            return false;
            throw new NotImplementedException();
        }
    }

    internal class NavigationSection : ISection
    {
        public Guid Id { get; } = Guid.NewGuid();
        public INavigationLink? Header { get; set; }
        public IEnumerable<INavigationLink>? Links { get; set; }
        public Boolean ShowDivider { get; set; }
        public Int32? MaxEntries { get; set; }
        public INavigationLink? Overflow { get ; set; }
    }
}
