/**
 * Photo Lightbox - Full-size photo viewing with keyboard and touch support
 */
(function () {
    'use strict';

    // State
    let currentPhotoIndex = 0;
    let photos = [];
    let isOpen = false;
    let touchStartX = 0;
    let touchEndX = 0;

    // DOM elements
    const lightbox = document.getElementById('photoLightbox');
    const lightboxImage = document.getElementById('lightbox-image');
    const lightboxCaption = document.getElementById('lightbox-caption');
    const lightboxDate = document.getElementById('lightbox-date');
    const lightboxLocation = document.getElementById('lightbox-location');
    const lightboxFilesize = document.getElementById('lightbox-filesize');
    const lightboxDimensions = document.getElementById('lightbox-dimensions');
    // We'll append camera info into the same metadata line using lightboxFilesize element or create new spans dynamically
    const lightboxCounter = document.getElementById('lightbox-counter-text');
    const lightboxLoading = document.querySelector('.lightbox-loading');
    const closeButton = document.querySelector('.lightbox-close');
    const prevButton = document.querySelector('.lightbox-prev');
    const nextButton = document.querySelector('.lightbox-next');
    const backdrop = document.querySelector('.lightbox-backdrop');

    /**
     * Initialize lightbox - gather photos and attach event listeners
     */
    function init() {
        // Gather all photos from the page
        const photoElements = document.querySelectorAll('[data-photo-id]');
        photos = Array.from(photoElements).map((el, index) => ({
            id: el.dataset.photoId,
            fileName: el.dataset.photoFilename || '',
            thumbnailUrl: el.dataset.photoThumbnail || '',
            fullSizeUrl: el.dataset.photoFullsize || '',
            location: el.dataset.photoLocation || null,
            dateTaken: el.dataset.photoDate || null,
            width: el.dataset.photoWidth ? parseInt(el.dataset.photoWidth) : null,
            height: el.dataset.photoHeight ? parseInt(el.dataset.photoHeight) : null,
            fileSize: el.dataset.photoFilesize || null,
            camera: el.dataset.photoCamera || null,
            aperture: el.dataset.photoAperture || null,
            shutter: el.dataset.photoShutter || null,
            iso: el.dataset.photoIso || null,
            focalLength: el.dataset.photoFocallength || null,
            index: index
        }));

        // Attach click handlers to all photos
        photoElements.forEach((el, index) => {
            el.style.cursor = 'pointer';
            el.addEventListener('click', function (e) {
                e.preventDefault();
                openLightbox(index);
            });
        });

        // Attach event listeners
        if (closeButton) closeButton.addEventListener('click', closeLightbox);
        if (backdrop) backdrop.addEventListener('click', closeLightbox);
        if (prevButton) prevButton.addEventListener('click', showPrevious);
        if (nextButton) nextButton.addEventListener('click', showNext);

        // Keyboard navigation
        document.addEventListener('keydown', handleKeyboard);

        // Touch gestures
        if (lightbox) {
            lightbox.addEventListener('touchstart', handleTouchStart, false);
            lightbox.addEventListener('touchend', handleTouchEnd, false);
        }
    }

    /**
     * Open lightbox with specified photo index
     */
    function openLightbox(index) {
        currentPhotoIndex = index;
        isOpen = true;
        
        if (lightbox) {
            lightbox.style.display = 'block';
            document.body.style.overflow = 'hidden'; // Prevent scrolling
        }

        loadPhoto(currentPhotoIndex);
        updateNavigation();
    }

    /**
     * Close lightbox
     */
    function closeLightbox() {
        isOpen = false;
        
        if (lightbox) {
            lightbox.style.display = 'none';
            document.body.style.overflow = ''; // Restore scrolling
        }
    }

    /**
     * Load and display photo at specified index
     */
    function loadPhoto(index) {
        if (index < 0 || index >= photos.length) return;

        const photo = photos[index];

        // Show loading indicator
        showLoading(true);

        // Create new image to preload
        const img = new Image();
        
        img.onload = function () {
            // Update image
            if (lightboxImage) {
                lightboxImage.src = photo.fullSizeUrl;
                lightboxImage.alt = photo.caption || photo.fileName;
            }

            // Update metadata
            updateMetadata(photo);
            
            // Hide loading indicator
            showLoading(false);
        };

        img.onerror = function () {
            // Fallback to same URL if full-size fails (no separate thumbnails used)
            if (lightboxImage) {
                lightboxImage.src = photo.fullSizeUrl;
                lightboxImage.alt = photo.caption || photo.fileName;
            }
            showLoading(false);
        };

        // Start loading
        img.src = photo.fullSizeUrl;
    }

    /**
     * Update metadata display
     */
    function updateMetadata(photo) {
        // Use filename as caption
        if (lightboxCaption) {
            lightboxCaption.textContent = photo.fileName;
            lightboxCaption.style.display = 'block';
        }

        // Date taken
        if (lightboxDate) {
            if (photo.dateTaken) {
                const date = new Date(photo.dateTaken);
                lightboxDate.innerHTML = `<i class="bi bi-calendar3"></i> ${date.toLocaleDateString()}`;
                lightboxDate.style.display = 'inline-block';
            } else {
                lightboxDate.style.display = 'none';
            }
        }

        // Location
        if (lightboxLocation) {
            if (photo.location) {
                lightboxLocation.innerHTML = `<i class="bi bi-geo-alt"></i> ${photo.location}`;
                lightboxLocation.style.display = 'inline-block';
            } else {
                lightboxLocation.style.display = 'none';
            }
        }

        // File size and camera info combined
        if (lightboxFilesize) {
            const bits = [];
            if (photo.fileSize) bits.push(`<i class="bi bi-file-earmark"></i> ${photo.fileSize}`);
            const camBits = [];
            if (photo.camera) camBits.push(photo.camera);
            if (photo.aperture) camBits.push(photo.aperture);
            if (photo.shutter) camBits.push(photo.shutter);
            if (photo.iso) camBits.push(`ISO ${photo.iso}`);
            if (photo.focalLength) camBits.push(photo.focalLength);
            if (camBits.length) bits.push(`<i class="bi bi-camera"></i> ${camBits.join(' • ')}`);
            if (bits.length) {
                lightboxFilesize.innerHTML = bits.join(' &nbsp;&nbsp; ');
                lightboxFilesize.style.display = 'inline-block';
            } else {
                lightboxFilesize.style.display = 'none';
            }
        }

        // Dimensions
        if (lightboxDimensions) {
            if (photo.width && photo.height) {
                lightboxDimensions.innerHTML = `<i class="bi bi-arrows-angle-expand"></i> ${photo.width} × ${photo.height}`;
                lightboxDimensions.style.display = 'inline-block';
            } else {
                lightboxDimensions.style.display = 'none';
            }
        }

        // Counter
        if (lightboxCounter) {
            lightboxCounter.textContent = `${photo.index + 1} / ${photos.length}`;
        }
    }

    /**
     * Show/hide loading indicator
     */
    function showLoading(show) {
        if (lightboxLoading) {
            lightboxLoading.style.display = show ? 'flex' : 'none';
        }
        if (lightboxImage) {
            lightboxImage.style.opacity = show ? '0.3' : '1';
        }
    }

    /**
     * Update navigation button visibility
     */
    function updateNavigation() {
        if (prevButton) {
            prevButton.style.display = currentPhotoIndex > 0 ? 'flex' : 'none';
        }
        if (nextButton) {
            nextButton.style.display = currentPhotoIndex < photos.length - 1 ? 'flex' : 'none';
        }
    }

    /**
     * Show previous photo
     */
    function showPrevious() {
        if (currentPhotoIndex > 0) {
            currentPhotoIndex--;
            loadPhoto(currentPhotoIndex);
            updateNavigation();
        }
    }

    /**
     * Show next photo
     */
    function showNext() {
        if (currentPhotoIndex < photos.length - 1) {
            currentPhotoIndex++;
            loadPhoto(currentPhotoIndex);
            updateNavigation();
        }
    }

    /**
     * Handle keyboard events
     */
    function handleKeyboard(e) {
        if (!isOpen) return;

        switch (e.key) {
            case 'Escape':
                closeLightbox();
                break;
            case 'ArrowLeft':
                e.preventDefault();
                showPrevious();
                break;
            case 'ArrowRight':
                e.preventDefault();
                showNext();
                break;
        }
    }

    /**
     * Handle touch start
     */
    function handleTouchStart(e) {
        touchStartX = e.changedTouches[0].screenX;
    }

    /**
     * Handle touch end (swipe detection)
     */
    function handleTouchEnd(e) {
        touchEndX = e.changedTouches[0].screenX;
        handleSwipe();
    }

    /**
     * Detect swipe direction and navigate
     */
    function handleSwipe() {
        const swipeThreshold = 50; // Minimum distance for swipe
        const diff = touchStartX - touchEndX;

        if (Math.abs(diff) > swipeThreshold) {
            if (diff > 0) {
                // Swipe left - next photo
                showNext();
            } else {
                // Swipe right - previous photo
                showPrevious();
            }
        }
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
