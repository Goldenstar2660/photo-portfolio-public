using Microsoft.AspNetCore.Mvc;
using PhotoGallery.PublicWeb.Models;
using PhotoGallery.PublicWeb.Services;

namespace PhotoGallery.PublicWeb.ViewComponents;

public class NavigationViewComponent : ViewComponent
{
    private readonly INavigationService _navigationService;
    private readonly ILogger<NavigationViewComponent> _logger;

    public NavigationViewComponent(
        INavigationService navigationService,
        ILogger<NavigationViewComponent> logger)
    {
        _navigationService = navigationService;
        _logger = logger;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var viewModel = new NavigationViewModel
        {
            AlbumTopics = await _navigationService.GetAlbumTopicsAsync(),
            ActivePage = ViewContext.RouteData.Values["page"]?.ToString() ?? ""
        };

        return View(viewModel);
    }
}
