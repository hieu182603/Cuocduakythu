// Async Screen Loader
async function loadScreens() {
    const container = document.getElementById("app-container");
    const screensToLoad = [
        { id: "splash-screen", key: "splash", file: "screens/splash.html" },
        { id: "main-menu-screen", key: "menu", file: "screens/menu.html" },
        { id: "lobby-screen", key: "lobby", file: "screens/lobby.html" },
        { id: "gameplay-screen", key: "gameplay", file: "screens/gameplay.html" },
        { id: "victory-screen", key: "victory", file: "screens/victory.html?v=2" }
    ];

    const loadedScreens = await Promise.all(screensToLoad.map(async s => {
        const response = await fetch(s.file);
        if (!response.ok) throw new Error(`Failed to load screen template: ${s.file}`);
        const html = await response.text();
        return { ...s, html };
    }));

    for (const s of loadedScreens) {
        // Find settings-modal and insert screens before it to maintain container structure
        const settingsModal = document.getElementById("settings-modal");
        if (settingsModal) {
            settingsModal.insertAdjacentHTML("beforebegin", s.html);
        } else {
            container.insertAdjacentHTML("beforeend", s.html);
        }
        screens[s.key] = document.getElementById(s.id);
    }
}

// Settings management
function loadSettings() {
    if (localStorage.getItem("settings_music")) musicVol = parseInt(localStorage.getItem("settings_music"));
    if (localStorage.getItem("settings_sfx")) sfxVol = parseInt(localStorage.getItem("settings_sfx"));
    if (localStorage.getItem("settings_speed")) speedSetting = localStorage.getItem("settings_speed");
    if (localStorage.getItem("settings_vfx")) vfxEnabled = localStorage.getItem("settings_vfx") === "true";
}
function saveSettings() {
    localStorage.setItem("settings_music", musicVol);
    localStorage.setItem("settings_sfx", sfxVol);
    localStorage.setItem("settings_speed", speedSetting);
    localStorage.setItem("settings_vfx", vfxEnabled);
}

// Screen navigation helper
function showScreen(screenKey) {
    // Ignore stale reconnect/lobby callbacks once the match is finishing.
    // resetGameStates() clears this lock before an intentional replay.
    if (screenKey === "lobby" && typeof isGameEnding !== "undefined" && isGameEnding) {
        console.debug("Ignored lobby navigation while final results are active.");
        return;
    }

    Object.keys(screens).forEach(key => {
        if (screens[key]) {
            if(key === screenKey) {
                screens[key].classList.add("active");
            } else {
                screens[key].classList.remove("active");
            }
        }
    });

    // Ensure menu content is fully restored and visible when showing the menu screen
    if (screenKey === "menu") {
        const menuContent = document.querySelector(".menu-content");
        if (menuContent) {
            menuContent.style.opacity = "1";
            menuContent.style.pointerEvents = "auto";
        }
    }
}
