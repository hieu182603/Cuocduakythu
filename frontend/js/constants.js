// character database representing all 10 horse characters
const CHARACTER_DATABASE = [
    { id: "01", name: "Lộc Phát", color: "var(--c-locphat)", badge: "⚡", vfx: "vfx-lightning", icon: "🏇", image: "assets/characters/locphat.png" },
    { id: "02", name: "Bạch Mã", color: "var(--c-bachma)", badge: "❄️", vfx: "vfx-water", icon: "🐎", image: "assets/characters/bachma.png" },
    { id: "03", name: "Hỏa Long", color: "var(--c-hoalong)", badge: "🔥", vfx: "vfx-fire", icon: "🏇", image: "assets/characters/hoalong.png" },
    { id: "04", name: "Thiên Phong", color: "var(--c-thienphong)", badge: "☁️", vfx: "", icon: "🐎", image: "assets/characters/thienphong.png" },
    { id: "05", name: "Ánh Sáng", color: "var(--c-anhsang)", badge: "✨", vfx: "", icon: "🏇", image: "assets/characters/anhsang.png" },
    { id: "06", name: "Hắc Mã", color: "var(--c-hacma)", badge: "🌙", vfx: "", icon: "🐎", image: "assets/characters/hacma.png" },
    { id: "07", name: "Kim Tướng", color: "var(--c-kimtuong)", badge: "🛡️", vfx: "", icon: "🏇", image: "assets/characters/kimtuong.png" },
    { id: "08", name: "Băng Phong", color: "var(--c-bangphong)", badge: "💎", vfx: "vfx-ice", icon: "🐎", image: "assets/characters/bangphong.png" },
    { id: "09", name: "Sakura", color: "var(--c-sakura)", badge: "🌸", vfx: "vfx-sakura", icon: "🏇", image: "assets/characters/sakura.png" },
    { id: "10", name: "Lôi Phong", color: "var(--c-loiphong)", badge: "⚡", vfx: "", icon: "🐎", image: "assets/characters/loiphong.png", imageClass: "is-wide" }
];

// 41 Tiles layout mapping grid positions (START + 40 tiles)
const SPECIAL_TILES = {
    8: 'reward', 22: 'reward', 36: 'reward',
    1: 'trap', 3: 'trap', 10: 'trap', 18: 'trap', 25: 'trap', 27: 'trap', 34: 'trap', 39: 'trap',
    5: 'bomb', 16: 'bomb', 29: 'bomb', 37: 'bomb',
    14: 'wheel', 30: 'wheel'
};

function getGridPos(n) {
    if (n === 0) return { r: 8, c: 1 };
    if (n >= 1 && n <= 6) return { r: 8 - n, c: 1 };
    if (n >= 7 && n <= 21) return { r: 1, c: n - 6 };
    if (n >= 22 && n <= 27) return { r: n - 20, c: 15 };
    if (n === 28) return { r: 8, c: 15 };
    return { r: 8, c: 43 - n };
}

const TILE_COORDS = Array.from({ length: 41 }, (_, idx) => {
    if (idx === 0) {
        return { row: 8, col: 1, type: "start", name: "START / FINISH", icon: "" };
    }
    const type = SPECIAL_TILES[idx] || 'q';
    const pos = getGridPos(idx);
    return {
        row: pos.r,
        col: pos.c,
        type: type,
        name: type === 'q' ? 'Câu hỏi' : (type === 'reward' ? 'Thưởng' : (type === 'wheel' ? 'Vòng quay' : 'Bẫy')),
        icon: "",
        isCorner: idx === 6 || idx === 21 || idx === 28
    };
});

// Fallback questions database if JSON file is missing
const EMBEDDED_QUESTIONS = [
    { "question": "Thủ đô của Việt Nam là thành phố nào?", "answers": ["TP. Hồ Chí Minh", "Đà Nẵng", "Hà Nội", "Hải Phòng"], "correct": 2 },
    { "question": "Trái Đất tự quay quanh trục mất bao lâu?", "answers": ["12 giờ", "24 giờ", "365 ngày", "30 ngày"], "correct": 1 },
    { "question": "Số nguyên tố nhỏ nhất là số nào?", "answers": ["0", "1", "2", "3"], "correct": 2 },
    { "question": "Kim loại nào dẫn điện tốt nhất ở điều kiện thường?", "answers": ["Vàng", "Đồng", "Bạc", "Nhôm"], "correct": 2 },
    { "question": "Đất nước mặt trời mọc là quốc gia nào?", "answers": ["Hàn Quốc", "Trung Quốc", "Việt Nam", "Nhật Bản"], "correct": 3 }
];
