using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;

namespace Mauren.Web.Features.Shared
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly IOptionsMonitor<Routing> _optionsMonitor;

        public HomeController(IOptionsMonitor<Routing> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }

        public IActionResult Index()
        {
            String? controller = _optionsMonitor.CurrentValue.Controller;
            String? action = _optionsMonitor.CurrentValue.Action;

            if (string.IsNullOrWhiteSpace(controller) || string.IsNullOrWhiteSpace(action))
            {
                var rootFiles = string.Join("\n", System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory()));
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                var baseConfig = System.IO.File.Exists("appsettings.json") ? System.IO.File.ReadAllText("appsettings.json") : "MISSING";
                var devConfig = System.IO.File.Exists("appsettings.Development.json") ? System.IO.File.ReadAllText("appsettings.Development.json") : "MISSING";

                return Content($"ROUTING DIAGNOSTICS 2.0:\n" +
                               $"------------------------\n" +
                               $"Environment: {env}\n\n" +
                               $"Root Files:\n{rootFiles}\n\n" +
                               $"appsettings.json:\n{baseConfig}\n\n" +
                               $"appsettings.Development.json:\n{devConfig}");
            }

            // Safety check: Prevent infinite loops if configuration fails to load
            if (string.IsNullOrWhiteSpace(controller) || string.IsNullOrWhiteSpace(action))
            {
                // Fallback to a known safe route, or return an error message
                return Content("Routing configuration is missing or invalid.");
            }

            return RedirectToAction(action, controller);
        }
    }
}
