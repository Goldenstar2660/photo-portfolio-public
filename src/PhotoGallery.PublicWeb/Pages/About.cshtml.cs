using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using PhotoGallery.PublicWeb.Models;

namespace PhotoGallery.PublicWeb.Pages;

/// <summary>
/// Page model for About page - loads photographer profile from configuration
/// </summary>
public class AboutModel : PageModel
{
    private readonly IConfiguration _configuration;

    public AboutModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// About page content
    /// </summary>
    public AboutViewModel About { get; set; } = new();

    public void OnGet()
    {
        // Load About page configuration
        var aboutSection = _configuration.GetSection("AboutPage");
        
        About = new AboutViewModel
        {
            Name = aboutSection.GetValue<string>("Name") ?? "Photographer",
            Tagline = aboutSection.GetValue<string>("Tagline") ?? string.Empty,
            Biography = aboutSection.GetValue<string>("Biography") ?? string.Empty,
            ProfilePhotoUrl = aboutSection.GetValue<string>("ProfilePhotoUrl") ?? "/img/about-round.jpg",
            Email = aboutSection.GetValue<string>("Email"),
            Hobbies = new List<HobbyViewModel>()
        };

        // Load hobbies
        var hobbiesSection = aboutSection.GetSection("Hobbies");
        if (hobbiesSection.Exists())
        {
            About.Hobbies = hobbiesSection.Get<List<HobbyViewModel>>() ?? new List<HobbyViewModel>();
        }
    }
}
