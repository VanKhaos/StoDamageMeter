// ============================================
// STO Damage Meter - Main JavaScript
// ============================================

document.addEventListener('DOMContentLoaded', function() {
    
    // ============================================
    // Smooth Scrolling for Navigation Links
    // ============================================
    const navLinks = document.querySelectorAll('a[href^="#"]');
    
    navLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            const targetId = this.getAttribute('href');
            
            if (targetId === '#') return;
            
            const targetSection = document.querySelector(targetId);
            
            if (targetSection) {
                const navbarHeight = document.querySelector('.navbar').offsetHeight;
                const targetPosition = targetSection.offsetTop - navbarHeight;
                
                window.scrollTo({
                    top: targetPosition,
                    behavior: 'smooth'
                });
            }
        });
    });
    
    // ============================================
    // Mobile Menu Toggle
    // ============================================
    const mobileMenuToggle = document.querySelector('.mobile-menu-toggle');
    const navMenu = document.querySelector('.nav-menu');
    
    if (mobileMenuToggle && navMenu) {
        mobileMenuToggle.addEventListener('click', function() {
            navMenu.classList.toggle('active');
            this.classList.toggle('active');
        });
    }
    
    // ============================================
    // Navbar Background on Scroll
    // ============================================
    const navbar = document.querySelector('.navbar');
    
    window.addEventListener('scroll', function() {
        if (window.scrollY > 100) {
            navbar.style.background = 'rgba(10, 10, 10, 0.98)';
            navbar.style.boxShadow = '0 2px 20px rgba(91, 155, 213, 0.1)';
        } else {
            navbar.style.background = 'rgba(10, 10, 10, 0.95)';
            navbar.style.boxShadow = 'none';
        }
    });
    
    // ============================================
    // Screenshot Gallery Lightbox
    // ============================================
    const screenshotItems = document.querySelectorAll('.screenshot-item');
    const lightbox = document.getElementById('lightbox');
    const lightboxImg = document.getElementById('lightbox-img');
    const lightboxCaption = document.querySelector('.lightbox-caption');
    const lightboxClose = document.querySelector('.lightbox-close');
    const lightboxPrev = document.querySelector('.lightbox-prev');
    const lightboxNext = document.querySelector('.lightbox-next');
    
    let currentScreenshotIndex = 0;
    const screenshots = Array.from(screenshotItems);
    
    // Open Lightbox
    screenshotItems.forEach((item, index) => {
        item.addEventListener('click', function() {
            currentScreenshotIndex = index;
            openLightbox(index);
        });
    });
    
    function openLightbox(index) {
        const img = screenshots[index].querySelector('img');
        const label = screenshots[index].querySelector('.screenshot-label');
        
        lightboxImg.src = img.src;
        lightboxCaption.textContent = label ? label.textContent : '';
        lightbox.classList.add('active');
        
        // Prevent body scroll
        document.body.style.overflow = 'hidden';
    }
    
    // Close Lightbox
    function closeLightbox() {
        lightbox.classList.remove('active');
        document.body.style.overflow = '';
    }
    
    if (lightboxClose) {
        lightboxClose.addEventListener('click', closeLightbox);
    }
    
    // Close on background click
    lightbox.addEventListener('click', function(e) {
        if (e.target === lightbox) {
            closeLightbox();
        }
    });
    
    // Close on Escape key
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape' && lightbox.classList.contains('active')) {
            closeLightbox();
        }
    });
    
    // Previous Screenshot
    if (lightboxPrev) {
        lightboxPrev.addEventListener('click', function(e) {
            e.stopPropagation();
            currentScreenshotIndex = (currentScreenshotIndex - 1 + screenshots.length) % screenshots.length;
            openLightbox(currentScreenshotIndex);
        });
    }
    
    // Next Screenshot
    if (lightboxNext) {
        lightboxNext.addEventListener('click', function(e) {
            e.stopPropagation();
            currentScreenshotIndex = (currentScreenshotIndex + 1) % screenshots.length;
            openLightbox(currentScreenshotIndex);
        });
    }
    
    // Keyboard navigation (Arrow keys)
    document.addEventListener('keydown', function(e) {
        if (!lightbox.classList.contains('active')) return;
        
        if (e.key === 'ArrowLeft') {
            currentScreenshotIndex = (currentScreenshotIndex - 1 + screenshots.length) % screenshots.length;
            openLightbox(currentScreenshotIndex);
        } else if (e.key === 'ArrowRight') {
            currentScreenshotIndex = (currentScreenshotIndex + 1) % screenshots.length;
            openLightbox(currentScreenshotIndex);
        }
    });
    
    // ============================================
    // Lazy Loading for Images
    // ============================================
    const lazyImages = document.querySelectorAll('img[loading="lazy"]');
    
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.src; // Trigger load
                    observer.unobserve(img);
                }
            });
        });
        
        lazyImages.forEach(img => imageObserver.observe(img));
    }
    
    // ============================================
    // Scroll Reveal Animation (Optional)
    // ============================================
    const revealElements = document.querySelectorAll('.feature-card, .screenshot-item, .step');
    
    const revealObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
            }
        });
    }, {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    });
    
    revealElements.forEach(element => {
        element.style.opacity = '0';
        element.style.transform = 'translateY(30px)';
        element.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
        revealObserver.observe(element);
    });
    
    // ============================================
    // Download Button Click Tracking (Optional)
    // ============================================
    const downloadButtons = document.querySelectorAll('a[href*="releases"]');
    
    downloadButtons.forEach(button => {
        button.addEventListener('click', function() {
            console.log('Download button clicked');
            // Add analytics tracking here if needed
            // Example: gtag('event', 'download', { version: '1.2.2' });
        });
    });
    
    // ============================================
    // Scroll to Top on Logo Click
    // ============================================
    const navLogo = document.querySelector('.nav-brand');
    
    if (navLogo) {
        navLogo.addEventListener('click', function(e) {
            e.preventDefault();
            window.scrollTo({
                top: 0,
                behavior: 'smooth'
            });
        });
        navLogo.style.cursor = 'pointer';
    }
    
});

