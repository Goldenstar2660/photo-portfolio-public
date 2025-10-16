namespace PhotoGallery.PublicWeb.Models;

/// <summary>
/// View model for individual hobby/interest
/// </summary>
public class HobbyViewModel
{
    /// <summary>
    /// Hobby title (e.g., "Photography", "Biking")
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Description of the hobby
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Bootstrap icon class (e.g., "bi-camera", "bi-bicycle")
    /// </summary>
    public string IconClass { get; set; } = string.Empty;
}
