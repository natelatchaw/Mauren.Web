using Discord.Interactions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Mauren.Discord.Infrastructure.Interactions
{
    internal class InteractionCommandErrorHandlerOptions
    {
        [ConfigurationKeyName("InteractionCommandErrors")]
        public Dictionary<InteractionCommandError, InteractionCommandErrorMetadata> Metadata { get; set; } = [];
    }

    internal class InteractionCommandErrorMetadata
    {
        [ConfigurationKeyName("Title")]
        public String? Title { get; set; }

        [ConfigurationKeyName("Description")]
        public String? Description { get; set; }

        [ConfigurationKeyName("ImageUrl")]
        public Uri? ImageUrl { get; set; }
    }
}