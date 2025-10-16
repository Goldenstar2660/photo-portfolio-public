/**
 * Photo Grid - Lazy Loading for Photo Thumbnails
 */

(function() {
    'use strict';

    // Intersection Observer for lazy loading
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                const src = img.getAttribute('data-src');
                
                if (src) {
                    img.src = src;
                    img.classList.add('loaded');
                    img.removeAttribute('data-src');
                    observer.unobserve(img);
                }
            }
        });
    }, {
        rootMargin: '50px 0px',
        threshold: 0.01
    });

    // Initialize lazy loading when DOM is ready
    document.addEventListener('DOMContentLoaded', function() {
        const lazyImages = document.querySelectorAll('.lazy-load');
        
        lazyImages.forEach(img => {
            imageObserver.observe(img);
        });

        // Add click handlers for lightbox (Phase 5 - will be implemented later)
        const photoItems = document.querySelectorAll('.photo-item img');
        photoItems.forEach(img => {
            img.addEventListener('click', function() {
                // Placeholder for lightbox functionality (User Story 2 - Phase 5)
                console.log('Photo clicked:', this.getAttribute('data-photo-id'));
                // TODO: Open lightbox modal with full-size image
            });
        });
    });

    // Preload images on hover for better UX
    document.addEventListener('mouseover', function(e) {
        if (e.target.classList.contains('lazy-load')) {
            const fullSrc = e.target.getAttribute('data-full');
            if (fullSrc) {
                const preloadImg = new Image();
                preloadImg.src = fullSrc;
            }
        }
    }, true);

})();
