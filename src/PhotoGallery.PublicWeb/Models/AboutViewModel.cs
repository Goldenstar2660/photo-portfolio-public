namespace PhotoGallery.PublicWeb.Models;

/// <summary>
/// View model for the About page
/// </summary>
public class AboutViewModel
{
    /// <summary>
    /// Photographer's full name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Brief tagline or title
    /// </summary>
    public string Tagline { get; set; } = string.Empty;

    /// <summary>
    /// Full biography text
    /// </summary>
    public string Biography { get; set; } = string.Empty;

    /// <summary>
    /// URL to profile photo
    /// </summary>
    public string ProfilePhotoUrl { get; set; } = string.Empty;

    /// <summary>
    /// Email address (optional)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// List of hobbies and interests
    /// </summary>
    public List<HobbyViewModel> Hobbies { get; set; } = new();
}
