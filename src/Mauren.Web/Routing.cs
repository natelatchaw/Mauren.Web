using Microsoft.Extensions.Configuration;
using System;

namespace Mauren.Web
{
    public class Routing
    {
        [ConfigurationKeyName("Controller")]
        public String? Controller { get; set; }

        [ConfigurationKeyName("Action")]
        public String? Action { get; set; }
    }
}
