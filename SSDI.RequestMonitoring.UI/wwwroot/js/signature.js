window.initializeCanvas = function (canvasId) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    ctx.fillStyle = 'white';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    ctx.strokeStyle = '#000000';
    ctx.lineWidth = 2;
    ctx.lineCap = 'round';
    ctx.lineJoin = 'round';
};

window.drawOnCanvas = function (data) {
    const canvas = document.getElementById(data.canvasId);
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    ctx.beginPath();
    ctx.moveTo(data.fromX, data.fromY);
    ctx.lineTo(data.toX, data.toY);
    ctx.strokeStyle = data.color;
    ctx.lineWidth = data.width;
    ctx.stroke();
};

window.clearCanvas = function (canvasId) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    ctx.fillStyle = 'white';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
};

window.getCanvasDataURL = function (canvasId) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return '';

    return canvas.toDataURL('image/png');
};

window.updatePreview = function (sourceCanvasId, targetCanvasId) {
    const sourceCanvas = document.getElementById(sourceCanvasId);
    const targetCanvas = document.getElementById(targetCanvasId);

    if (!sourceCanvas || !targetCanvas) return;

    const targetCtx = targetCanvas.getContext('2d');
    targetCtx.fillStyle = 'white';
    targetCtx.fillRect(0, 0, targetCanvas.width, targetCanvas.height);
    targetCtx.drawImage(sourceCanvas, 0, 0, targetCanvas.width, targetCanvas.height);
};

window.getCanvasBoundingRect = function (canvasId) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) {
        return { left: 0, top: 0, width: 0, height: 0 };
    }

    const rect = canvas.getBoundingClientRect();
    return {
        left: rect.left,
        top: rect.top,
        width: rect.width,
        height: rect.height
    };
};

// Browser information helper
window.getBrowserInfo = function () {
    const userAgent = navigator.userAgent;
    let browser = "Unknown";

    if (userAgent.indexOf("Chrome") > -1) {
        browser = "Chrome";
    } else if (userAgent.indexOf("Firefox") > -1) {
        browser = "Firefox";
    } else if (userAgent.indexOf("Safari") > -1) {
        browser = "Safari";
    } else if (userAgent.indexOf("Edge") > -1) {
        browser = "Edge";
    } else if (userAgent.indexOf("Trident") > -1) {
        browser = "Internet Explorer";
    }

    return browser;
};

// Screen resolution helper
window.getScreenResolution = function () {
    return `${window.screen.width} × ${window.screen.height}`;
};