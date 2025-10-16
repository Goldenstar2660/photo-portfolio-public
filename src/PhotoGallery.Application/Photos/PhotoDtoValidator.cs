using FluentValidation;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using System;

namespace PhotoGallery.Photos
{
    public class CreatePhotoDtoValidator : AbstractValidator<CreatePhotoDto>
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
        private const long MinFileSize = 1024; // 1KB minimum
        private static readonly Regex ValidLocationRegex = new Regex(@"^[a-zA-Z0-9\s\-_\.,]+$", RegexOptions.Compiled);

        public CreatePhotoDtoValidator()
        {
            RuleFor(x => x.AlbumId)
                .NotEmpty()
                .WithMessage("Album ID is required");

            RuleFor(x => x.File)
                .NotNull()
                .WithMessage("File is required")
                .Must(file => file != null && file.Length > 0)
                .WithMessage("File cannot be empty")
                .Must(file => file == null || file.Length >= MinFileSize)
                .WithMessage($"File must be at least {MinFileSize / 1024}KB")
                .Must(file => file == null || file.Length <= MaxFileSize)
                .WithMessage($"File size cannot exceed {MaxFileSize / 1024 / 1024}MB")
                .Must(file => file == null || AllowedExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
                .WithMessage($"File must be one of the following types: {string.Join(", ", AllowedExtensions)}")
                .Must(file => file == null || AllowedMimeTypes.Contains(file.ContentType))
                .WithMessage("File content type is not supported")
                .Must(file => file == null || !string.IsNullOrEmpty(file.FileName))
                .WithMessage("File must have a valid filename")
                .Must(file => file == null || !file.FileName.Contains(".."))
                .WithMessage("Filename cannot contain parent directory references")
                .When(x => x.File != null);

            RuleFor(x => x.Location)
                .MaximumLength(100)
                .WithMessage("Location cannot exceed 100 characters")
                .Must(location => string.IsNullOrEmpty(location) || ValidLocationRegex.IsMatch(location))
                .WithMessage("Location can only contain letters, numbers, spaces, hyphens, underscores, periods, and commas")
                .Must(location => string.IsNullOrEmpty(location) || !string.IsNullOrWhiteSpace(location.Trim()))
                .WithMessage("Location cannot be only whitespace")
                .When(x => !string.IsNullOrEmpty(x.Location));

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Display order must be non-negative")
                .LessThan(10000)
                .WithMessage("Display order must be less than 10,000");
        }
    }
    public class UpdatePhotoDtoValidator : AbstractValidator<UpdatePhotoDto>
    {
        private static readonly Regex ValidLocationRegex = new Regex(@"^[a-zA-Z0-9\s\-_\.,]+$", RegexOptions.Compiled);

        public UpdatePhotoDtoValidator()
        {
            RuleFor(x => x.Location)
                .MaximumLength(100)
                .WithMessage("Location cannot exceed 100 characters")
                .Must(location => string.IsNullOrEmpty(location) || ValidLocationRegex.IsMatch(location))
                .WithMessage("Location can only contain letters, numbers, spaces, hyphens, underscores, periods, and commas")
                .Must(location => string.IsNullOrEmpty(location) || !string.IsNullOrWhiteSpace(location.Trim()))
                .WithMessage("Location cannot be only whitespace")
                .When(x => !string.IsNullOrEmpty(x.Location));

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Display order must be non-negative")
                .LessThan(10000)
                .WithMessage("Display order must be less than 10,000");
        }
    }

    public class ReorderPhotosDtoValidator : AbstractValidator<ReorderPhotosDto>
    {
        public ReorderPhotosDtoValidator()
        {
            RuleFor(x => x.PhotoOrders)
                .NotEmpty()
                .WithMessage("Photo orders list cannot be empty")
                .Must(orders => orders.Count <= 1000)
                .WithMessage("Cannot reorder more than 1,000 photos at once");

            RuleForEach(x => x.PhotoOrders)
                .SetValidator(new PhotoOrderDtoValidator());

            RuleFor(x => x.PhotoOrders)
                .Must(orders => orders.Select(o => o.Id).Distinct().Count() == orders.Count)
                .WithMessage("Duplicate photo IDs are not allowed in reorder operation");
        }
    }

    public class PhotoOrderDtoValidator : AbstractValidator<PhotoOrderDto>
    {
        public PhotoOrderDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Photo ID is required");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Display order must be non-negative")
                .LessThan(10000)
                .WithMessage("Display order must be less than 10,000");
        }
    }

    public class GetPhotosInputValidator : AbstractValidator<GetPhotosInput>
    {
        private static readonly Regex ValidLocationRegex = new Regex(@"^[a-zA-Z0-9\s\-_\.,]+$", RegexOptions.Compiled);

        public GetPhotosInputValidator()
        {
            RuleFor(x => x.Location)
                .MaximumLength(100)
                .WithMessage("Location filter cannot exceed 100 characters")
                .Must(location => string.IsNullOrEmpty(location) || ValidLocationRegex.IsMatch(location))
                .WithMessage("Location filter can only contain letters, numbers, spaces, hyphens, underscores, periods, and commas")
                .When(x => !string.IsNullOrEmpty(x.Location));

            RuleFor(x => x.SearchTerm)
                .MaximumLength(100)
                .WithMessage("Search term cannot exceed 100 characters")
                .Must(term => string.IsNullOrEmpty(term) || !string.IsNullOrWhiteSpace(term.Trim()))
                .WithMessage("Search term cannot be only whitespace")
                .When(x => !string.IsNullOrEmpty(x.SearchTerm));

            RuleFor(x => x.DateTakenFrom)
                .LessThanOrEqualTo(x => x.DateTakenTo)
                .WithMessage("Date taken from must be less than or equal to date taken to")
                .LessThanOrEqualTo(DateTime.Now.AddDays(1))
                .WithMessage("Date taken from cannot be in the future")
                .When(x => x.DateTakenFrom.HasValue && x.DateTakenTo.HasValue);

            RuleFor(x => x.DateTakenTo)
                .LessThanOrEqualTo(DateTime.Now.AddDays(1))
                .WithMessage("Date taken to cannot be in the future")
                .When(x => x.DateTakenTo.HasValue);

            RuleFor(x => x.MaxResultCount)
                .GreaterThan(0)
                .WithMessage("Maximum result count must be greater than 0")
                .LessThanOrEqualTo(1000)
                .WithMessage("Maximum result count cannot exceed 1000");

            RuleFor(x => x.SkipCount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Skip count must be non-negative");
        }
    }

    public class GetRandomPhotosInputValidator : AbstractValidator<GetRandomPhotosInput>
    {
        public GetRandomPhotosInputValidator()
        {
            RuleFor(x => x.Count)
                .GreaterThan(0)
                .WithMessage("Count must be greater than 0")
                .LessThanOrEqualTo(50)
                .WithMessage("Count cannot exceed 50");

            RuleFor(x => x.ExcludeAlbumId)
                .NotEmpty()
                .WithMessage("Exclude album ID must be valid")
                .When(x => x.ExcludeAlbumId.HasValue);
        }
    }
}