/**
 * Custom notification toast system
 */
function showNotification(message, type = "info", duration = 3000) {
    let container = document.getElementById("toast-container");
    if (!container) {
        container = document.createElement("div");
        container.id = "toast-container";
        container.className = "toast-container";
        document.body.appendChild(container);
    }

    const toast = document.createElement("div");
    toast.className = `toast ${type}`;

    let iconHtml = '<i class="fa-solid fa-info-circle toast-icon" style="color: #3b82f6;"></i>';
    if (type === "success") {
        iconHtml = '<i class="fa-solid fa-circle-check toast-icon" style="color: #10b981;"></i>';
    } else if (type === "error") {
        iconHtml = '<i class="fa-solid fa-circle-exclamation toast-icon" style="color: #ef4444;"></i>';
    } else if (type === "warning") {
        iconHtml = '<i class="fa-solid fa-triangle-exclamation toast-icon" style="color: #f59e0b;"></i>';
    }

    toast.innerHTML = `
        ${iconHtml}
        <span class="toast-message">${message}</span>
    `;

    container.appendChild(toast);

    // Trigger transition
    setTimeout(() => {
        toast.classList.add("show");
    }, 10);

    // Remove toast after duration
    setTimeout(() => {
        toast.classList.remove("show");
        setTimeout(() => {
            toast.remove();
        }, 300);
    }, duration);
}

/**
 * Custom confirmation modal dialog
 */
function showConfirm(message, onConfirm) {
    const modal = document.getElementById("custom-confirm-modal");
    if (!modal) {
        if (confirm(message)) {
            onConfirm();
        }
        return;
    }

    document.getElementById("confirm-modal-message").innerText = message;
    modal.classList.add("active");

    const btnYes = document.getElementById("btn-confirm-yes");
    const btnNo = document.getElementById("btn-confirm-no");
    const overlay = document.getElementById("confirm-modal-overlay");

    const cleanup = () => {
        modal.classList.remove("active");
    };

    if (btnYes) {
        btnYes.onclick = () => {
            cleanup();
            onConfirm();
        };
    }

    if (btnNo) {
        btnNo.onclick = () => {
            cleanup();
        };
    }

    if (overlay) {
        overlay.onclick = () => {
            cleanup();
        };
    }
}
