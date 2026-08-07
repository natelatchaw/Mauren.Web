using Mauren.Discord.Application.Features.Connection;
using Microsoft.AspNetCore.Mvc;

namespace Mauren.Discord.UI.Features.Connection.Views.Components.GuildTile
{
    [ViewComponent]
    public class GuildTileViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(GuildMetadataViewModel viewModel) => View(viewModel);
    }
}
