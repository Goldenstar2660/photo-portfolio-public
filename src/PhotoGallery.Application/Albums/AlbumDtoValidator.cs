using FluentValidation;
using System.Text.RegularExpressions;
using System.Linq;

namespace PhotoGallery.Albums
{
    public class CreateAlbumDtoValidator : AbstractValidator<CreateAlbumDto>
    {
        private static readonly Regex ValidNameRegex = new Regex(@"^[a-zA-Z0-9\s\-_\.]+$", RegexOptions.Compiled);
        private static readonly Regex ValidTopicRegex = new Regex(@"^[a-zA-Z0-9\s\-_]+$", RegexOptions.Compiled);

        public CreateAlbumDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Album name is required")
                .Length(1, 100)
                .WithMessage("Album name must be between 1 and 100 characters")
                .Must(name => ValidNameRegex.IsMatch(name))
                .WithMessage("Album name can only contain letters, numbers, spaces, hyphens, underscores, and periods")
                .Must(name => !string.IsNullOrWhiteSpace(name?.Trim()))
                .WithMessage("Album name cannot be only whitespace");

            RuleFor(x => x.Topic)
                .NotEmpty()
                .WithMessage("Album topic is required")
                .Length(1, 50)
                .WithMessage("Album topic must be between 1 and 50 characters")
                .Must(topic => ValidTopicRegex.IsMatch(topic))
                .WithMessage("Album topic can only contain letters, numbers, spaces, hyphens, and underscores")
                .Must(topic => !string.IsNullOrWhiteSpace(topic?.Trim()))
                .WithMessage("Album topic cannot be only whitespace");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Display order must be non-negative")
                .LessThan(10000)
                .WithMessage("Display order must be less than 10,000");

            RuleFor(x => x.CoverImagePath)
                .MaximumLength(500)
                .WithMessage("Cover image path cannot exceed 500 characters")
                .Must(path => string.IsNullOrEmpty(path) || !path.Contains(".."))
                .WithMessage("Cover image path cannot contain parent directory references")
                .When(x => !string.IsNullOrEmpty(x.CoverImagePath));
        }
    }

    public class UpdateAlbumDtoValidator : AbstractValidator<UpdateAlbumDto>
    {
        private static readonly Regex ValidNameRegex = new Regex(@"^[a-zA-Z0-9\s\-_\.]+$", RegexOptions.Compiled);
        private static readonly Regex ValidTopicRegex = new Regex(@"^[a-zA-Z0-9\s\-_]+$", RegexOptions.Compiled);

        public UpdateAlbumDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Album name is required")
                .Length(1, 100)
                .WithMessage("Album name must be between 1 and 100 characters")
                .Must(name => ValidNameRegex.IsMatch(name))
                .WithMessage("Album name can only contain letters, numbers, spaces, hyphens, underscores, and periods")
                .Must(name => !string.IsNullOrWhiteSpace(name?.Trim()))
                .WithMessage("Album name cannot be only whitespace");

            RuleFor(x => x.Topic)
                .NotEmpty()
                .WithMessage("Album topic is required")
                .Length(1, 50)
                .WithMessage("Album topic must be between 1 and 50 characters")
                .Must(topic => ValidTopicRegex.IsMatch(topic))
                .WithMessage("Album topic can only contain letters, numbers, spaces, hyphens, and underscores")
                .Must(topic => !string.IsNullOrWhiteSpace(topic?.Trim()))
                .WithMessage("Album topic cannot be only whitespace");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Display order must be non-negative")
                .LessThan(10000)
                .WithMessage("Display order must be less than 10,000");

            RuleFor(x => x.CoverImagePath)
                .MaximumLength(500)
                .WithMessage("Cover image path cannot exceed 500 characters")
                .Must(path => string.IsNullOrEmpty(path) || !path.Contains(".."))
                .WithMessage("Cover image path cannot contain parent directory references")
                .When(x => !string.IsNullOrEmpty(x.CoverImagePath));
        }
    }

    public class ReorderAlbumsDtoValidator : AbstractValidator<ReorderAlbumsDto>
    {
        public ReorderAlbumsDtoValidator()
        {
            RuleFor(x => x.AlbumOrders)
                .NotEmpty()
                .WithMessage("Album orders list cannot be empty")
                .Must(orders => orders.Count <= 1000)
                .WithMessage("Cannot reorder more than 1,000 albums at once");

            RuleForEach(x => x.AlbumOrders)
                .SetValidator(new AlbumOrderDtoValidator());

            RuleFor(x => x.AlbumOrders)
                .Must(orders => orders.Select(o => o.Id).Distinct().Count() == orders.Count)
                .WithMessage("Duplicate album IDs are not allowed in reorder operation");
        }
    }

    public class AlbumOrderDtoValidator : AbstractValidator<AlbumOrderDto>
    {
        public AlbumOrderDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Album ID is required");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Display order must be non-negative")
                .LessThan(10000)
                .WithMessage("Display order must be less than 10,000");
        }
    }
}