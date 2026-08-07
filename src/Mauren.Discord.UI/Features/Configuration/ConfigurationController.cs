using Mauren.Discord.Application.Abstractions.Messaging;
using Mauren.Discord.Application.Abstractions.Validation;
using Mauren.Discord.Application.Features.Configuration;
using Mauren.Discord.Application.Features.Plugins;
using Mauren.Discord.Core;
using Mauren.Discord.UI.Features.Configuration.Models;
using Mauren.Discord.UI.Features.Plugins.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Mauren.Discord.UI.Features.Configuration
{
    [Route("Discord/[controller]")]
    public class ConfigurationController : Controller
    {
        private readonly ILogger<ConfigurationController> _logger;
        private readonly IDispatcher _dispatcher;

        public ConfigurationController(ILogger<ConfigurationController> logger, IDispatcher dispatcher)
        {
            _logger = logger;
            _dispatcher = dispatcher;
        }

        [HttpGet("")]
        [HttpGet("[action]")]
        public async Task<IActionResult> Index()
        {
            return RedirectToAction(nameof(Status));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Status()
        {
            // Create a new query to get the bot state
            GetBotWorkerStatusQuery query = new GetBotWorkerStatusQuery();

            // Dispatch the query to the pipeline
            Result<Boolean> result = await _dispatcher.DispatchAsync<GetBotWorkerStatusQuery, Boolean>(query);

            result.TryGetValue(out Boolean isRunning);

            return View(model: new StatusViewModel
            {
                IsRunning = isRunning,
            });
        }

        [HttpPost("start-bot")]
        public async Task<IActionResult> StartBot()
        {
            // Create a new command to resume the bot
            SetBotWorkerStatusCommand command = SetBotWorkerStatusCommand.Resume;

            // Dispatch the command to the pipeline
            await _dispatcher.DispatchAsync<SetBotWorkerStatusCommand>(command);

            return RedirectToAction(nameof(Status));
        }

        [HttpPost("stop-bot")]
        public async Task<IActionResult> StopBot()
        {
            // Create a new command to pause the bot
            SetBotWorkerStatusCommand command = SetBotWorkerStatusCommand.Pause;

            // Dispatch the command to the pipeline
            await _dispatcher.DispatchAsync<SetBotWorkerStatusCommand>(command);

            return RedirectToAction(nameof(Status));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Token(CancellationToken cancellationToken)
        {
            try
            {
                // Create a new query to get the bot token
                GetTokenQuery query = new();

                // Dispatch the query to the pipeline
                Result<TokenInformation?> result = await _dispatcher.DispatchAsync<GetTokenQuery, TokenInformation?>(query, cancellationToken);

                result.TryGetValue(out TokenInformation? tokenInformation);

                // Return the view
                return View(model: new TokenViewModel
                {
                    CurrentToken = tokenInformation?.Value switch
                    {
                        String value => new String('*', value.Length),
                        _ => null,
                    },
                    NewToken = default,
                    LastUpdated = tokenInformation?.LastUpdated,
                });
            }
            catch (ValidationException exception)
            {
                foreach (ValidationError error in exception.Errors)
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

                return View(model: new TokenViewModel
                {

                });
            }
        }

        [HttpPost("[action]")]
        //[Route("Token")]
        public async Task<IActionResult> Token([FromForm] String newToken, CancellationToken cancellationToken)
        {
            try
            {
                // Create a new command to update the bot token
                UpdateTokenCommand command = new(newToken);

                // Dispatch the command to the pipeline
                await _dispatcher.DispatchAsync<UpdateTokenCommand>(command, cancellationToken);

                TempData["Success"] = "Token updated successfully.";
                return RedirectToAction(nameof(Token));
            }
            catch (ValidationException exception)
            {
                foreach (ValidationError error in exception.Errors)
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

                return View(model: new TokenViewModel
                {

                });
            }
        }
    }
}
