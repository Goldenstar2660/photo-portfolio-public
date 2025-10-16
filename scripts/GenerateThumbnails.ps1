# PhotoGallery Thumbnail Generator Script
# This script generates thumbnails for all images in the photos folder
# Supports subfolder structure: photos/{album}/ -> thumbnails/{album}/

param(
    [string]$PhotosPath = "src\PhotoGallery.PublicWeb\wwwroot\uploads\photos",
    [string]$ThumbnailsPath = "src\PhotoGallery.PublicWeb\wwwroot\uploads\thumbnails",
    [int]$ThumbnailWidth = 600,
    [int]$ThumbnailHeight = 400,
    [string]$AlbumFilter = "",  # Optional: process only specific album subfolder
    [switch]$Debug = $false     # Show detailed debug information
)

# Load System.Drawing assembly
Add-Type -AssemblyName System.Drawing

# Get the absolute paths
$photosFullPath = Join-Path $PSScriptRoot "..\$PhotosPath"
$thumbnailsFullPath = Join-Path $PSScriptRoot "..\$ThumbnailsPath"

Write-Host "Photos Path: $photosFullPath" -ForegroundColor Green
Write-Host "Thumbnails Path: $thumbnailsFullPath" -ForegroundColor Green

# Create thumbnails directory if it doesn't exist
if (!(Test-Path $thumbnailsFullPath)) {
    New-Item -ItemType Directory -Path $thumbnailsFullPath -Force
    Write-Host "Created thumbnails directory: $thumbnailsFullPath" -ForegroundColor Yellow
}

# Check if photos directory exists
if (!(Test-Path $photosFullPath)) {
    Write-Host "Photos directory not found: $photosFullPath" -ForegroundColor Red
    Write-Host "Please create the directory and add your photos first." -ForegroundColor Red
    exit 1
}

# Get album subfolders or use filter
$albumFolders = @()
if ($AlbumFilter -ne "") {
    $albumPath = Join-Path $photosFullPath $AlbumFilter
    if (Test-Path $albumPath) {
        $albumFolders = @(Get-Item $albumPath)
        Write-Host "Processing specific album: $AlbumFilter" -ForegroundColor Cyan
    } else {
        Write-Host "Album folder not found: $AlbumFilter" -ForegroundColor Red
        exit 1
    }
} else {
    $albumFolders = Get-ChildItem -Path $photosFullPath -Directory
    if ($albumFolders.Count -eq 0) {
        Write-Host "No album subfolders found in: $photosFullPath" -ForegroundColor Red
        Write-Host "Expected structure: photos/{album_name}/" -ForegroundColor Red
        exit 1
    }
    Write-Host "Found $($albumFolders.Count) album folders to process..." -ForegroundColor Green
}

$totalImages = 0
$totalThumbnails = 0

foreach ($albumFolder in $albumFolders) {
    $albumName = $albumFolder.Name
    $albumPhotosPath = $albumFolder.FullName
    $albumThumbnailsPath = Join-Path $thumbnailsFullPath $albumName
    
    Write-Host "`nProcessing album: $albumName" -ForegroundColor Cyan
    if ($Debug) {
        Write-Host "Album path: $albumPhotosPath" -ForegroundColor DarkGray
    }
    
    # Create album thumbnail directory if it doesn't exist
    if (!(Test-Path $albumThumbnailsPath)) {
        New-Item -ItemType Directory -Path $albumThumbnailsPath -Force | Out-Null
        Write-Host "Created album thumbnails directory: $albumThumbnailsPath" -ForegroundColor Yellow
    }
    
    # Debug: List all files in the album folder
    if ($Debug) {
        $allFiles = Get-ChildItem -Path $albumPhotosPath -File
        Write-Host "Total files in album folder: $($allFiles.Count)" -ForegroundColor DarkGray
        if ($allFiles.Count -gt 0) {
            Write-Host "File extensions found: $($allFiles | ForEach-Object { $_.Extension } | Sort-Object -Unique)" -ForegroundColor DarkGray
        }
    }
    
    # Get all image files in this album folder
    $imageFiles = Get-ChildItem -Path $albumPhotosPath -File | Where-Object { $_.Extension -match '(?i)\.(jpg|jpeg|png|bmp|gif)$' }

    
    if ($imageFiles.Count -eq 0) {
        Write-Host "No image files found in album: $albumName" -ForegroundColor Yellow
        continue
    }
    
    Write-Host "Found $($imageFiles.Count) image files in album: $albumName" -ForegroundColor Green
    $totalImages += $imageFiles.Count
    $albumThumbnailCount = 0

    foreach ($imageFile in $imageFiles) {
        try {
            # Create thumbnail filename (keep original filename, just change location)
            $thumbnailPath = Join-Path $albumThumbnailsPath $imageFile.Name
            
            # Skip if thumbnail already exists
            if (Test-Path $thumbnailPath) {
                Write-Host "  Thumbnail already exists: $($imageFile.Name)" -ForegroundColor Yellow
                continue
            }
            
            # Load the original image
            $originalImage = [System.Drawing.Image]::FromFile($imageFile.FullName)
            
            # Calculate thumbnail dimensions while maintaining aspect ratio
            $originalWidth = $originalImage.Width
            $originalHeight = $originalImage.Height
            
            $ratioX = $ThumbnailWidth / $originalWidth
            $ratioY = $ThumbnailHeight / $originalHeight
            $ratio = [Math]::Min($ratioX, $ratioY)
            
            $newWidth = [int]($originalWidth * $ratio)
            $newHeight = [int]($originalHeight * $ratio)
            
            # Create thumbnail
            $thumbnail = New-Object System.Drawing.Bitmap($newWidth, $newHeight)
            $graphics = [System.Drawing.Graphics]::FromImage($thumbnail)
            
            # Set high quality rendering
            $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
            $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
            
            # Draw the resized image
            $graphics.DrawImage($originalImage, 0, 0, $newWidth, $newHeight)
            
            # Save thumbnail
            $thumbnail.Save($thumbnailPath, $originalImage.RawFormat)
            
            Write-Host "  Created thumbnail: $($imageFile.Name) ($newWidth x $newHeight)" -ForegroundColor Green
            $albumThumbnailCount++
            
            # Clean up
            $graphics.Dispose()
            $thumbnail.Dispose()
            $originalImage.Dispose()
            
        } catch {
            Write-Host "  Error processing $($imageFile.Name): $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    
    $totalThumbnails += $albumThumbnailCount
    Write-Host "Album $albumName completed: $albumThumbnailCount thumbnails created" -ForegroundColor Green
}

Write-Host "`nThumbnail generation completed!" -ForegroundColor Green
Write-Host "Total images processed: $totalImages" -ForegroundColor Green
Write-Host "Total thumbnails created: $totalThumbnails" -ForegroundColor Green
Write-Host "Thumbnails organized by album in: $thumbnailsFullPath" -ForegroundColor Green

# Usage examples
Write-Host "`nUsage examples:" -ForegroundColor Cyan
Write-Host "  Process all albums:        .\GenerateThumbnails.ps1" -ForegroundColor White
Write-Host "  Process specific album:    .\GenerateThumbnails.ps1 -AlbumFilter 'maritimes'" -ForegroundColor White
Write-Host "  Custom dimensions:         .\GenerateThumbnails.ps1 -ThumbnailWidth 400 -ThumbnailHeight 300" -ForegroundColor White
Write-Host "  Debug mode:                .\GenerateThumbnails.ps1 -Debug" -ForegroundColor White