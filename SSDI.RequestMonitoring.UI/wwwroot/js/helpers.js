window.addResizeListener = function (dotNetHelper) {
    function updateSize() {
        dotNetHelper.invokeMethodAsync('SetIsMobile', window.innerWidth);
    }

    window.addEventListener('resize', updateSize);
    updateSize();
}
//=============================================================

// Scroll function for system config table
function scrollTableToBottom() {
    // Find the table body
    const tableBody = document.querySelector('.sysconfig-table-body');
    if (!tableBody) return;

    // Smooth scroll to bottom
    tableBody.scrollTo({
        top: tableBody.scrollHeight,
        behavior: 'smooth'
    });

    // Find new items and highlight them
    const newItems = document.querySelectorAll('.new-item');
    if (newItems.length > 0) {
        // Remove any existing highlights
        newItems.forEach(item => {
            item.classList.remove('highlight-pulse');
        });

        // Add highlight class to trigger animation
        setTimeout(() => {
            newItems.forEach(item => {
                item.classList.add('highlight-pulse');
            });
        }, 300);
    }
}

// Make function available globally
window.scrollTableToBottom = scrollTableToBottom;

//=============================================================



window.scrollToElement = function (element) {
    if (element) {
        element.scrollIntoView({
            behavior: 'smooth',
            block: 'start'
        });
    }
};

window.scrollToTopOfModal = function () {
    // Find the modal container and scroll it to top
    const modal = document.querySelector('.modal__overlay[style*="display: flex"] .modal');
    if (modal) {
        modal.scrollTop = 0;
    }
};