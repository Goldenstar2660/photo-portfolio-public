using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PhotoGallery.PublicWeb.Models;
using PhotoGallery.PublicWeb.Services;

namespace PhotoGallery.PublicWeb.Pages;

public class AlbumsModel : PageModel
{
    private readonly IAlbumsPageService _albumsPageService;
    private readonly ILogger<AlbumsModel> _logger;

    public AlbumsModel(
        IAlbumsPageService albumsPageService,
        ILogger<AlbumsModel> logger)
    {
        _albumsPageService = albumsPageService;
        _logger = logger;
    }

    public List<AlbumCardViewModel> Albums { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    [BindProperty(SupportsGet = true)]
    public string? Topic { get; set; }

    public async Task OnGetAsync(int? page)
    {
        PageNumber = page ?? 1;
        
        _logger.LogInformation("Loading albums page {Page}, filtered by topic: {Topic}", PageNumber, Topic ?? "all");

        var result = await _albumsPageService.GetAlbumsAsync(PageNumber, PageSize, Topic);
        
        Albums = result.Albums;
        TotalCount = result.TotalCount;

        _logger.LogInformation("Albums page loaded with {Count} albums (total: {Total})", Albums.Count, TotalCount);
    }
}
