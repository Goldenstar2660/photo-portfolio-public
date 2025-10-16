using Microsoft.AspNetCore.Mvc.RazorPages;
using PhotoGallery.PublicWeb.Models;
using PhotoGallery.PublicWeb.Services;

namespace PhotoGallery.PublicWeb.Pages;

public class IndexModel : PageModel
{
    private readonly IHomePageService _homePageService;
    private readonly ILogger<IndexModel> _logger;

    public HomeViewModel HomeData { get; set; } = new();

    public IndexModel(
        IHomePageService homePageService,
        ILogger<IndexModel> logger)
    {
        _homePageService = homePageService;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        _logger.LogInformation("Loading home page...");
        HomeData = await _homePageService.GetHomePageDataAsync();
        _logger.LogInformation("Home page loaded with {AlbumCount} featured albums and {PhotoCount} hero photos",
            HomeData.FeaturedAlbums.Count, HomeData.HeroPhotos.Count);
    }
}

