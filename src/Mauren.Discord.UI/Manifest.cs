using Mauren.Discord.UI.Features;

namespace Mauren.Discord.UI
{
    internal static class Manifest
    {
        internal static NavigationLink Header => new()
        {
            Identifier = "bot.header",
            Label = "Bot",
            Area = "Discord",
            Controller = "Configuration",
            Action = null,
            IconName = null,
            Title = "Bot Management"
        };

        internal static class Configuration
        {
            public static NavigationLink Header => new()
            {
                Identifier = "bot.configuration.header",
                Label = "Configuration",
                Controller = "Configuration",
                Action = "Index",
                IconName = "bi-gear-wide-connected",
                Title = "Configuration Manager",
            };

            public static NavigationLink Status => new()
            {
                Identifier = "bot.configuration.status",
                Label = "Status",
                Controller = "Configuration",
                Action = "Status",
                IconName = "bi-cpu",
                Title = "Status Overview",
                ButtonColor = Bootstrap.ButtonColor.Primary,
                ButtonStyle = Bootstrap.ButtonStyle.Normal,
            };

            public static NavigationLink Token => new()
            {
                Identifier = "bot.configuration.token",
                Label = "Token",
                Controller = "Configuration",
                Action = "Token",
                IconName = "bi-key",
                Title = "Change Bot Token",
                ButtonColor = Bootstrap.ButtonColor.Primary,
                ButtonStyle = Bootstrap.ButtonStyle.Normal,
            };
        }

        internal static class Connection
        {
            public static NavigationLink Header => new()
            {
                Identifier = "bot.connection.header",
                Label = "Connection",
                Controller = "Connection",
                Action = "Index",
                IconName = "bi-reception-4",
                Title = "Connection Manager",
            };

            public static NavigationLink Status => new()
            {
                Identifier = "bot.connection.status",
                Label = "Status",
                Controller = "Connection",
                Action = "Status",
                IconName = "bi-globe2",
                Title = "Connection Status Overview",
                ButtonColor = Bootstrap.ButtonColor.Primary,
                ButtonStyle = Bootstrap.ButtonStyle.Normal,
            };

            public static NavigationLink Guilds => new()
            {
                Identifier = "bot.connection.guilds",
                Label = "Guilds",
                Controller = "Connection",
                Action = "Guilds",
                IconName = "bi-people-fill",
                Title = "Connected Guilds",
                ButtonColor = Bootstrap.ButtonColor.Primary,
                ButtonStyle = Bootstrap.ButtonStyle.Normal,
            };
        }

        internal static class Plugins
        {
            public static NavigationLink Header => new()
            {
                Identifier = "bot.plugins.header",
                Label = "Plugins",
                Controller = "Plugins",
                Action = "Index",
                IconName = "bi-puzzle-fill",
                Title = "Plugin Manager",
            };

            public static NavigationLink Installed => new()
            {
                Identifier = "bot.plugins.installed",
                Label = "Installed Plugins",
                Controller = "Plugins",
                Action = "Installed",
                IconName = "bi-hdd",
                Title = "Installed Plugins Overview",
                ButtonColor = Bootstrap.ButtonColor.Primary,
                ButtonStyle = Bootstrap.ButtonStyle.Normal,
            };

            public static NavigationLink Upload => new()
            {
                Identifier = "bot.plugins.upload",
                Label = "Upload Plugins",
                Controller = "Plugins",
                Action = "Upload",
                IconName = "bi-upload",
                Title = "Upload Plugin",
                ButtonColor = Bootstrap.ButtonColor.Primary,
                ButtonStyle = Bootstrap.ButtonStyle.Normal,
            };

            public static NavigationLink Commands => new()
            {
                Identifier = "bot.plugins.commands",
                Label = "Commands",
                Controller = "Plugins",
                Action = "Commands",
                IconName = "bi-slash-square",
                Title = "View Commands",
                ButtonColor = Bootstrap.ButtonColor.Primary,
                ButtonStyle = Bootstrap.ButtonStyle.Normal,
            };
        }
    }
}
