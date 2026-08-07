using Microsoft.AspNetCore.Mvc;

namespace Mauren.Discord.UI.Features.Shared.Views.Components.TitleCard
{
    [ViewComponent]
    public class TitleCardViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(String title, String? subtitle = default) => View(new Data
        {
            Title = title,
            Subtitle = subtitle,
        });
    }

    internal struct Data
    {
        public required String Title { get; set; }
        public String? Subtitle { get; set; }
    }
}
