using Bootstrap.Interfaces;
using Mauren.AspNetCore.Mvc.Navigation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Mauren.Discord.UI.Features.Configuration
{
    internal static class Navigation
    {
        internal static IMenu? Menu => new NavigationMenu
        {
            Header = Manifest.Configuration.Header,
            Sections = new List<NavigationSection>
            {
                // General Section
                new NavigationSection
                {
                    Header = null,
                    Links = new List<NavigationLink>
                    {
                        Manifest.Configuration.Status,
                        Manifest.Configuration.Token,
                    },
                    ShowDivider = true,
                }
            },
            ShowDivider = true,
        };
    }

    internal class NavigationHeader : INavigationLink
    {
        public required String Identifier { get; set; }
        public String? Label { get; set; }
        public String? IconName { get; set; }
        public Bootstrap.LinkColor? LinkColor { get; }
        public Bootstrap.ButtonColor? ButtonColor { get; }
        public Bootstrap.ButtonStyle? ButtonStyle { get; }
        public IEnumerable<KeyValuePair<String, String?>> Properties { get; }
        public String? Area { get; }
        public String? Page { get; }
        public String? Handler { get; }
        public String? Controller { get; }
        public String? Action { get; }
        public Dictionary<String, String>? RouteValues { get; }
        public String? Title { get; }

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
}
