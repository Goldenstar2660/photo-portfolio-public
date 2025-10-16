using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PhotoGallery.PublicWeb.Models;
using PhotoGallery.PublicWeb.Services;

namespace PhotoGallery.PublicWeb.Pages;

public class AlbumModel : PageModel
{
    private readonly IAlbumPageService _albumPageService;
    private readonly ILogger<AlbumModel> _logger;

    public AlbumViewModel? Album { get; set; }
    public bool AlbumNotFound { get; set; }
    public BreadcrumbViewModel Breadcrumbs { get; set; } = new();

    public AlbumModel(
        IAlbumPageService albumPageService,
        ILogger<AlbumModel> logger)
    {
        _albumPageService = albumPageService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(Guid? id, int page = 1)
    {
        if (id == null)
        {
            _logger.LogWarning("Album ID is required");
            return RedirectToPage("/Index");
        }

        _logger.LogInformation("Loading album {AlbumId}, page {Page}", id, page);
        
        Album = await _albumPageService.GetAlbumByIdAsync(id.Value, page);
        
        if (Album == null)
        {
            _logger.LogWarning("Album {AlbumId} not found", id);
            AlbumNotFound = true;
            
            // Breadcrumbs for not found page
            Breadcrumbs.Items = new List<BreadcrumbItemViewModel>
            {
                new() { Text = "Home", Url = "/" },
                new() { Text = "Albums", Url = "/Albums", IsActive = false },
                new() { Text = "Not Found", Url = "", IsActive = true }
            };
            
            return Page();
        }

        // Build breadcrumbs: Home > Albums > Album Name
        Breadcrumbs.Items = new List<BreadcrumbItemViewModel>
        {
            new() { Text = "Home", Url = "/" },
            new() { Text = "Albums", Url = "/Albums", IsActive = false },
            new() { Text = Album.Name, Url = "", IsActive = true }
        };

        _logger.LogInformation("Album {AlbumName} loaded with {PhotoCount} photos", 
            Album.Name, Album.Photos.Count);
        
        return Page();
    }
}
