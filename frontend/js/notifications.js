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

    let iconClass = "fa-info-circle";
    let iconColor = "#3b82f6";
    if (type === "success") {
        iconClass = "fa-circle-check";
        iconColor = "#10b981";
    } else if (type === "error") {
        iconClass = "fa-circle-exclamation";
        iconColor = "#ef4444";
    } else if (type === "warning") {
        iconClass = "fa-triangle-exclamation";
        iconColor = "#f59e0b";
    }

    const icon = document.createElement("i");
    icon.className = `fa-solid ${iconClass} toast-icon`;
    icon.style.color = iconColor;
    const messageNode = document.createElement("span");
    messageNode.className = "toast-message";
    messageNode.textContent = String(message ?? "");
    toast.append(icon, messageNode);

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
