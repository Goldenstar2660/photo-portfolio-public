using PhotoGallery;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Volo.Abp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseAutofac();
await builder.AddApplicationAsync<PhotoGalleryPublicWebModule>();

var app = builder.Build();
await app.InitializeApplicationAsync();

// Dynamic image resize endpoint: /img/{path}?w=...&h=...&fit=crop|contain&fmt=webp|jpeg
app.MapGet("/img/{**path}", async (string path, int? w, int? h, string? fit, string? fmt, IMemoryCache cache, IWebHostEnvironment env) =>
{
	// Sanitize input path: must be under wwwroot/uploads/photos
	var uploadsRoot = Path.Combine(env.ContentRootPath, "wwwroot", "uploads", "photos");
	var requested = path.Replace("\\", "/");
	if (requested.StartsWith("/")) requested = requested[1..];
	var absPath = Path.GetFullPath(Path.Combine(uploadsRoot, requested));
	if (!absPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase) || !File.Exists(absPath))
	{
		return Results.NotFound();
	}

	int width = w.GetValueOrDefault(400); // sensible default for grid
	int height = h.GetValueOrDefault(300);
	string fitMode = string.IsNullOrWhiteSpace(fit) ? "crop" : fit.ToLowerInvariant();
	string format = string.IsNullOrWhiteSpace(fmt) ? "webp" : fmt.ToLowerInvariant();

	// Cache key based on file + parameters + last write time
	var fi = new FileInfo(absPath);
	var cacheKey = $"img::{absPath}::{width}x{height}::{fitMode}::{format}::{fi.LastWriteTimeUtc:O}";
	if (cache.TryGetValue(cacheKey, out byte[]? cachedBytes) && cachedBytes is not null)
	{
		return Results.File(cachedBytes, contentType: format == "jpeg" ? "image/jpeg" : "image/webp");
	}

	// Process image
	using var image = await Image.LoadAsync(absPath);
	var resizeOptions = new ResizeOptions
	{
		Size = new Size(width, height),
		Mode = fitMode == "contain" ? ResizeMode.Pad : ResizeMode.Crop,
		Sampler = KnownResamplers.Lanczos3
	};
	image.Mutate(x => x.Resize(resizeOptions));

	await using var ms = new MemoryStream();
	if (format == "jpeg")
	{
		var encoder = new JpegEncoder { Quality = 80 };
		await image.SaveAsync(ms, encoder);
	}
	else
	{
		var encoder = new WebpEncoder { Quality = 80 };
		await image.SaveAsync(ms, encoder);
	}
	var bytes = ms.ToArray();
	cache.Set(cacheKey, bytes, new MemoryCacheEntryOptions
	{
		SlidingExpiration = TimeSpan.FromMinutes(20),
		AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
	});

	return Results.File(bytes, contentType: format == "jpeg" ? "image/jpeg" : "image/webp");
});

await app.RunAsync();
