using Mauren.Discord.Application.Features.Plugins;
using Microsoft.AspNetCore.Mvc;

namespace Mauren.Discord.UI.Features.Plugins.Views.Components.PluginTile
{
    [ViewComponent]
    public class PluginTileViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(PluginMetadataViewModel viewModel) => View(viewModel);
    }
}
