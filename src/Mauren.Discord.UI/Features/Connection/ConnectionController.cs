using Discord;
using Mauren.Discord.Application.Abstractions.Messaging;
using Mauren.Discord.Application.Features.Connection;
using Mauren.Discord.UI.Features.Connection.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Mauren.Discord.UI.Features.Connection
{
    [Route("Discord/[controller]")]
    public class ConnectionController : Controller
    {
        private readonly ILogger<ConnectionController> _logger;
        private readonly IDispatcher _dispatcher;

        public ConnectionController(ILogger<ConnectionController> logger, IDispatcher dispatcher)
        {
            _logger = logger;
            _dispatcher = dispatcher;
        }

        [HttpGet("")]
        [HttpGet("[action]")]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Status));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Status(CancellationToken cancellationToken)
        {
            return View(model: new StatusViewModel
            {

            });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Guilds(CancellationToken cancellationToken)
        {
            // Create a new query to get the list of currently connected guilds
            GetGuildsQuery query = new();

            // Dispatch the query to the pipeline
            Result<IEnumerable<IGuild>> result = await _dispatcher
                .DispatchAsync<GetGuildsQuery, IEnumerable<IGuild>>(query, cancellationToken);

            // Try to get the guilds collection from the result
            result.TryGetValue(out IEnumerable<IGuild>? guilds);
            if (result.Error is String error)
            {
                ModelState.AddModelError(String.Empty, error);
            }

            IList<GuildMetadataViewModel> viewModels = [];
            foreach (IGuild guild in guilds ?? [])
            {
                GuildMetadataViewModel viewModel = await guild.AsViewModel();
                viewModels.Add(viewModel);
            }

            return View(model: new GuildsViewModel
            {
                Guilds = viewModels
            });
        }
    }
}
