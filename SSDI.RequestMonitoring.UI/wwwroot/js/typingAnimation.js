function typeWriterLoop(elementId, text, speed = 100, pauseTime = 2000) {
    const element = document.getElementById(elementId);
    if (!element) {
        console.error(`Element with id '${elementId}' not found`);
        return;
    }

    function type() {
        // Reset and start typing immediately
        element.textContent = '';
        let index = 0;
        
        function typeCharacter() {
            if (index < text.length) {
                element.textContent += text.charAt(index);
                index++;
                setTimeout(typeCharacter, speed);
            } else {
                // Finished typing, wait then restart
                setTimeout(type, pauseTime);
            }
        }
        
        // Start typing
        typeCharacter();
    }

    // Start the infinite loop
    type();
}

function initializeTypingAnimation() {
    typeWriterLoop('animated-text', 'Request Monitoring', 100, 2000);
}

// Make functions globally available
window.typeWriterLoop = typeWriterLoop;
window.initializeTypingAnimation = initializeTypingAnimation;