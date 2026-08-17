using Discord;
using Mauren.Discord.Application.Abstractions.Messaging;
using Mauren.Discord.Application.Abstractions.Validation;
using Mauren.Discord.Application.Features.Plugins;
using Mauren.Discord.Core;
using Mauren.Discord.UI.Features.Configuration.Models;
using Mauren.Discord.UI.Features.Plugins.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Mauren.Discord.UI.Features.Plugins
{
    [Route("Discord/[controller]")]
    public class PluginsController : Controller
    {
        private readonly ILogger<PluginsController> _logger;
        private readonly IDispatcher _dispatcher;

        public PluginsController(ILogger<PluginsController> logger, IDispatcher dispatcher)
        {
            _logger = logger;
            _dispatcher = dispatcher;
        }

        [HttpGet("")]
        [HttpGet("[action]")]
        public async Task<IActionResult> Index()
        {
            return RedirectToAction(nameof(Installed));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Location(CancellationToken cancellationToken)
        {
            try
            {
                // Create a new query to get the plugin directory
                GetLocationQuery query = new();

                // Dispatch the query to the pipeline
                Result<LocationInformation?> result = await _dispatcher.DispatchAsync<GetLocationQuery, LocationInformation?>(query, cancellationToken);

                result.TryGetValue(out LocationInformation? locationInformation);

                // Return the view
                return View(model: new LocationViewModel
                {
                    CurrentPath = locationInformation?.Value switch
                    {
                        String value => value,
                        _ => null,
                    },
                    NewPath = default,
                    LastUpdated = null,
                });
            }
            catch (ValidationException exception)
            {
                foreach (ValidationError error in exception.Errors)
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

                return View(model: new LocationViewModel
                {

                });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Location([FromForm] String newPath, CancellationToken cancellationToken)
        {
            try
            {
                // Create a new command to update the plugin directory location
                UpdateLocationCommand command = new(newPath);

                // Dispatch the command to the pipeline
                await _dispatcher.DispatchAsync<UpdateLocationCommand>(command, cancellationToken);

                TempData["Success"] = "Location updated successfully.";
                return RedirectToAction(nameof(Location));
            }
            catch (ValidationException exception)
            {
                foreach (ValidationError error in exception.Errors)
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

                return View(model: new LocationViewModel
                {

                });
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Installed(CancellationToken cancellationToken)
        {
            // Build the view model
            InstalledViewModel viewModel = await BuildInstalledViewModelAsync(cancellationToken);

            return View(model: viewModel);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Upload(CancellationToken cancellationToken)
        {
            // Build the view model
            UploadViewModel viewModel = new();

            return View(model: viewModel);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Commands(CancellationToken cancellationToken)
        {
            // Build the view model
            CommandsViewModel viewModel = await BuildCommandsViewModelAsync(cancellationToken);

            return View(model: viewModel);
        }

        [RequestSizeLimit(100 * 1024 * 1024)]     // Allows up to 100MB Total Body
        [RequestFormLimits(MultipartBodyLengthLimit = 100 * 1024 * 1024)] // Allows up to 100MB File Size
        [HttpPost("[action]")]
        public async Task<IActionResult> UploadAsync([FromForm] UploadViewModel viewModel, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(viewModel, nameof(viewModel));
            try
            {
                // If the file is null or empty
                if (viewModel?.File is not IFormFile file || file.Length == 0)
                {
                    // Create a new validation error
                    ValidationError error = new(nameof(viewModel.File), "Invalid file uploaded.");
                    throw new ValidationException(error);
                }

                // Construct a new memory stream
                using MemoryStream stream = new();
                // Copy the file to the memory stream
                await file.CopyToAsync(stream, cancellationToken);
                // Seek to the beginning of the memory stream
                stream.Seek(0, SeekOrigin.Begin);

                // Create a new command to upload the plugin
                UploadPluginCommand command = new(file.FileName, file.ContentType, stream);

                // Dispatch the command to the pipeline
                await _dispatcher.DispatchAsync<UploadPluginCommand>(command);

                return RedirectToAction(nameof(Installed));
            }
            catch (ValidationException exception)
            {
                foreach (ValidationError error in exception.Errors)
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

                // Repopulate the view model
                UploadViewModel reloadedViewModel = new();
                // Preserve user input
                reloadedViewModel.File = viewModel.File;

                // Explicitly return the "Installed" view with the repopulated view model
                return View(nameof(Installed), reloadedViewModel);
            }
            catch (Exception exception)
            {
                // Catch-all for non-validation errors (e.g., file system locks, missing directories)
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.Log(LogLevel.Error, exception, "Failed to upload plugin '{file}'", viewModel.File?.FileName);

                ModelState.AddModelError(String.Empty, exception.Message);

                return RedirectToAction(nameof(Upload));
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> RemoveAsync([FromForm] UploadViewModel viewModel, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(viewModel.PluginId, nameof(viewModel.PluginId));
            try
            {
                // Create a new command to remove the plugin
                RemovePluginCommand command = new(viewModel.PluginId);

                // Dispatch the command to the pipeline
                Result result = await _dispatcher.DispatchAsync<RemovePluginCommand>(command, cancellationToken);

                if (result.Error is String error)
                    throw new InvalidOperationException(error);
                
                // Redirect to the Installed page on success
                return RedirectToAction(nameof(Installed));
            }
            catch (ValidationException exception)
            {
                foreach (ValidationError error in exception.Errors)
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

                // Repopulate the view model
                UploadViewModel reloadedViewModel = new();

                // Explicitly return the "Upload" view with the repopulated view model
                return View(nameof(Upload), reloadedViewModel);
            }
            catch (Exception exception)
            {
                // Catch-all for non-validation errors (e.g., file system locks, missing directories)
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.Log(LogLevel.Error, exception, "Failed to remove plugin '{PluginId}'", viewModel.PluginId);

                ModelState.AddModelError(String.Empty, exception.Message);

                return RedirectToAction(nameof(Upload));
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> SyncCommandsAsync([FromForm] UInt64? guildId, CancellationToken cancellationToken)
        {
            try
            {
                // Create a new command to sync the loaded plugins to Discord
                SyncPluginsCommand command = new(guildId);

                // Dispatch the command to the pipeline
                Result result = await _dispatcher.DispatchAsync<SyncPluginsCommand>(command, cancellationToken);

                if (result.Error is String error)
                    throw new InvalidOperationException(error);

                TempData["SuccessMessage"] = guildId.HasValue switch
                {
                    true => $"Successfully synchronized commands to guild {guildId.Value}.",
                    false => "Successfully synchronized commands globally.",
                };

                return RedirectToAction(nameof(Commands));
            }
            catch (Exception exception)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.Log(LogLevel.Error, exception, "Failed to sync commands to Discord");

                TempData["ErrorMessage"] = $"Sync failed: {exception.Message}";

                return RedirectToAction(nameof(Commands));
            }
        }
        
        /// <summary>
        /// Helper method to centralize fetching plugins, guilds, and mapping them to view models.
        /// </summary>
        private async Task<InstalledViewModel> BuildInstalledViewModelAsync(CancellationToken cancellationToken)
        {
            // Create a new query to get the plugin collection
            GetPluginsQuery pluginsQuery = new();

            // Dispatch the plugins query to the pipeline
            Result<IEnumerable<PluginMetadata>> pluginsResult = await _dispatcher
                .DispatchAsync<GetPluginsQuery, IEnumerable<PluginMetadata>>(pluginsQuery, cancellationToken);

            // Try to get the plugins collection from the result
            pluginsResult.TryGetValue(out IEnumerable<PluginMetadata>? plugins);
            if (pluginsResult.Error is String pluginsError)
            {
                ModelState.AddModelError(String.Empty, pluginsError);
            }

            IList<PluginMetadataViewModel> pluginViewModels = [];
            foreach (PluginMetadata plugin in plugins ?? [])
            {
                PluginMetadataViewModel pluginViewModel = await plugin.AsViewModel();
                pluginViewModels.Add(pluginViewModel);
            }

            return new InstalledViewModel
            {
                Plugins = pluginViewModels,
            };
        }


        private async Task<CommandsViewModel> BuildCommandsViewModelAsync(CancellationToken cancellationToken)
        {
            // Create a new query to get the guild collection
            GetGuildsQuery guildsQuery = new();

            // Dispatch the guilds query to the pipeline
            Result<IEnumerable<IGuild>> guildsResult = await _dispatcher
                .DispatchAsync<GetGuildsQuery, IEnumerable<IGuild>>(guildsQuery, cancellationToken);

            // Try to get the guilds collection from the result
            guildsResult.TryGetValue(out IEnumerable<IGuild>? guilds);
            if (guildsResult.Error is String guildsError)
            {
                ModelState.AddModelError(String.Empty, guildsError);
            }

            IList<GuildMetadataViewModel> guildViewModels = [];
            foreach (IGuild guild in guilds ?? [])
            {
                GuildMetadataViewModel guildViewModel = await guild.AsViewModel();
                guildViewModels.Add(guildViewModel);
            }

            return new CommandsViewModel
            {
                Guilds = guildViewModels,
            };
        }
    }
}