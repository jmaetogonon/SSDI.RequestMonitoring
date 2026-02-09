window.autocomplete = {
    initClickOutside: (dotNetRef, element) => {
        const handleClickOutside = (event) => {
            setTimeout(async () => {
                const container = element.closest('.autocomplete-container');

                if (!container) {
                    safeInvoke(dotNetRef);
                    return;
                }

                const clickedInside = container.contains(event.target);
                const isClearButton = event.target.closest('.autocomplete-clear-btn');

                if (!clickedInside && !isClearButton) {
                    safeInvoke(dotNetRef);
                }
            }, 10);
        };

        document.addEventListener('mousedown', handleClickOutside);
        element._clickOutsideHandler = handleClickOutside;
    },

    removeClickOutside: (element) => {
        if (element?._clickOutsideHandler) {
            document.removeEventListener('mousedown', element._clickOutsideHandler);
            delete element._clickOutsideHandler;
        }
    }
};

async function safeInvoke(dotNetRef) {
    try {
        await dotNetRef.invokeMethodAsync('HandleClickOutside');
    } catch {
        // Component already disposed — ignore
    }
}
