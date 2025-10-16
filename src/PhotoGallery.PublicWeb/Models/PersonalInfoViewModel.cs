namespace PhotoGallery.PublicWeb.Models;

/// <summary>
/// Base view model containing personal/photographer information
/// Used across pages that need to display photographer details
/// </summary>
public class PersonalInfoViewModel
{
    public string Name { get; set; } = "Photographer Name";
    public string Email { get; set; } = "contact@photogallery.com";
    public string Phone { get; set; } = "+1 (555) 123-4567";
    public string Bio { get; set; } = string.Empty;
    public string ProfileImageUrl { get; set; } = "/img/about.jpg";
    
    // Social media links
    public string? FacebookUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? LinkedInUrl { get; set; }
}
