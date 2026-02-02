// Autocomplete component click outside detection
window.autocomplete = {
    initClickOutside: (dotNetRef, element) => {
        const handleClickOutside = (event) => {
            // Use setTimeout to let mousedown events finish
            setTimeout(() => {
                const autocompleteContainer = element.closest('.autocomplete-container');

                if (!autocompleteContainer) {
                    dotNetRef.invokeMethodAsync('HandleClickOutside');
                    return;
                }

                // Check if click is inside the autocomplete container
                const clickedInside = autocompleteContainer.contains(event.target);

                // Only close if click is outside AND not on the clear button
                const isClearButton = event.target.closest('.autocomplete-clear-btn');

                if (!clickedInside && !isClearButton) {
                    dotNetRef.invokeMethodAsync('HandleClickOutside');
                }
            }, 10);
        };

        // Use mousedown instead of click to work with stopPropagation
        document.addEventListener('mousedown', handleClickOutside);

        // Store reference for cleanup
        element._clickOutsideHandler = handleClickOutside;
    },

    removeClickOutside: (element) => {
        if (element._clickOutsideHandler) {
            document.removeEventListener('mousedown', element._clickOutsideHandler);
            delete element._clickOutsideHandler;
        }
    }
};