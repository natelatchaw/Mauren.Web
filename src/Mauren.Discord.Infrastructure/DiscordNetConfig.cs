namespace Discord.Net
{
    internal class DiscordNetConfig
    {
        public Discord.WebSocket.DiscordSocketConfig? DiscordSocketConfig { get; set; }
        public Discord.Commands.CommandServiceConfig? CommandServiceConfig { get; set; }
        public Discord.Interactions.InteractionServiceConfig? InteractionServiceConfig { get; set; }
    }
}
