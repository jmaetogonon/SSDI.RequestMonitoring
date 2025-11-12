// Create blob URL from base64 data
window.createBlobUrl = (base64Data, contentType) => {
    try {
        // Convert base64 to binary
        const binaryString = atob(base64Data);
        const bytes = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }

        const blob = new Blob([bytes], { type: contentType });
        return URL.createObjectURL(blob);
    } catch (error) {
        console.error('Error creating blob URL:', error);
        return null;
    }
};

// Revoke blob URL to free memory
window.revokeBlobUrl = (url) => {
    try {
        if (url && url.startsWith('blob:')) {
            URL.revokeObjectURL(url);
        }
    } catch (error) {
        console.error('Error revoking blob URL:', error);
    }
};

// Save file from base64
window.saveAsFile = (fileName, base64Data) => {
    try {
        const link = document.createElement('a');
        link.download = fileName;
        link.href = 'data:application/octet-stream;base64,' + base64Data;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    } catch (error) {
        console.error('Error saving file:', error);
    }
};

//download pdf
window.downloadBase64File = function (contentType, base64Data, fileName) {
    const linkSource = `data:${contentType};base64,${base64Data}`;
    const downloadLink = document.createElement("a");
    downloadLink.href = linkSource;
    downloadLink.download = fileName;
    downloadLink.click();
};