const carouselMap = new WeakMap();

export function initializeCarousel(container, dotNetHelper) {
    if (!container) return;

    let touchStartX = 0;
    let touchEndX = 0;
    const swipeThreshold = 50;

    const state = {
        dotNetRef: dotNetHelper,
        onTouchStart: (e) => {
            touchStartX = e.changedTouches[0].screenX;
        },
        onTouchEnd: (e) => {
            touchEndX = e.changedTouches[0].screenX;
            const diff = touchStartX - touchEndX;

            if (Math.abs(diff) > swipeThreshold && state.dotNetRef) {
                if (diff > 0) {
                    state.dotNetRef.invokeMethodAsync("NextSlideJS");
                } else {
                    state.dotNetRef.invokeMethodAsync("PrevSlideJS");
                }
            }
        }
    };

    container.addEventListener("touchstart", state.onTouchStart, { passive: true });
    container.addEventListener("touchend", state.onTouchEnd, { passive: true });

    carouselMap.set(container, state);
}

export function disposeCarousel(container) {
    const state = carouselMap.get(container);
    if (!state) return;

    container.removeEventListener("touchstart", state.onTouchStart);
    container.removeEventListener("touchend", state.onTouchEnd);

    state.dotNetRef = null;
    carouselMap.delete(container);
}
