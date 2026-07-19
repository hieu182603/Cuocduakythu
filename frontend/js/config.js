// =============================================
// DEPLOYMENT CONFIGURATION
// =============================================
// Tự động phát hiện môi trường:
// - localhost → dùng backend local (port 5089)
// - production (Vercel) → dùng VPS backend URL

const APP_CONFIG = (() => {
    const hostname = window.location.hostname;
    const isLocal = hostname === "localhost" || hostname === "127.0.0.1";

    if (isLocal) {
        return {
            // Khi chạy local dev
            SIGNALR_URL: `http://${hostname}:5089/gameHub`,
            API_BASE: `http://${hostname}:5089`,
        };
    }

    // ====================================================================
    // 🔴 THAY ĐỔI URL NÀY THÀNH DOMAIN/IP CỦA VPS CỦA BẠN
    // Ví dụ: "https://api.cuocduakythu.com" hoặc "http://123.45.67.89:5089"
    // ====================================================================
    const VPS_BACKEND_URL = "https://cuocduakythu-chi-backend-hieu182603.onrender.com";

    return {
        SIGNALR_URL: `${VPS_BACKEND_URL}/gameHub`,
        API_BASE: VPS_BACKEND_URL,
    };
})();
