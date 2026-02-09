export function initializeCarousel(dotNetRef) {
    const container = document.querySelector('.metrics-carousel');
    if (!container) return;

    // Store the .NET reference globally
    window.__dashboardDotNetHelper = dotNetRef;

    container.setAttribute('data-carousel', 'true');

    let touchStartX = 0;
    let touchEndX = 0;
    const swipeThreshold = 50;

    container.addEventListener('touchstart', (e) => {
        touchStartX = e.changedTouches[0].screenX;
    }, { passive: true });

    container.addEventListener('touchend', (e) => {
        touchEndX = e.changedTouches[0].screenX;
        handleSwipe();
    }, { passive: true });

    function handleSwipe() {
        const diff = touchStartX - touchEndX;

        if (Math.abs(diff) > swipeThreshold) {
            if (diff > 0) {
                // Swipe left - next slide
                window.__dashboardDotNetHelper?.invokeMethodAsync('NextSlideJS');
            } else {
                // Swipe right - previous slide
                window.__dashboardDotNetHelper?.invokeMethodAsync('PrevSlideJS');
            }
        }
    }
}

export function disposeCarousel() {
    window.__dashboardDotNetHelper = null;
}
