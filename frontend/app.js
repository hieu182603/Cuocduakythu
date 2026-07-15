// ==========================================
// GAME ENTRYPOINT & EVENT LISTENERS
// ==========================================

// Initialization
document.addEventListener("DOMContentLoaded", () => {
    loadScreens()
        .then(() => {
            loadSettings();
            initApp();
            initStaticEvents();

            // Global click sound effect listener
            document.addEventListener("click", (e) => {
                if (e.target && (e.target.tagName === "BUTTON" || e.target.closest("button") || e.target.classList.contains("event-card"))) {
                    if (typeof playSFX === "function") {
                        playSFX("click");
                    }
                }
            });

            // Prevent accidental page reload (F5) or closing the tab during gameplay/lobby
            window.addEventListener("beforeunload", (e) => {
                const gameplayActive = screens.gameplay && screens.gameplay.classList.contains("active");
                const lobbyActive = screens.lobby && screens.lobby.classList.contains("active");
                
                if (gameplayActive || lobbyActive) {
                    e.preventDefault();
                    e.returnValue = "Trận đấu hoặc phòng chờ đang diễn ra. Bạn có chắc muốn rời đi?";
                    return e.returnValue;
                }
            });
        })
        .catch(err => {
            console.error("Critical Error loading game screens: ", err);
        });
});

function safeAddListener(id, event, handler) {
    const el = document.getElementById(id);
    if (el) {
        el.addEventListener(event, handler);
    } else {
        console.warn(`[SafeListener] Element #${id} not found, skipping event listener.`);
    }
}

function initApp() {
    // 1. Load Questions
    fetch("questions.json")
        .then(res => res.json())
        .then(data => {
            questions = data;
            const qCount = document.getElementById("qpool-count");
            if (qCount) qCount.innerText = questions.length;
        })
        .catch(err => {
            console.warn("Could not load questions.json, using fallback database.", err);
            questions = EMBEDDED_QUESTIONS;
            const qCount = document.getElementById("qpool-count");
            if (qCount) qCount.innerText = questions.length;
        });

    // 2. Setup Screen Transitions
    setTimeout(() => {
        showScreen("menu");
    }, 2200); // Wait for splash animation

    const btnPlay = document.getElementById("btn-play");
    if (btnPlay) {
        btnPlay.addEventListener("click", () => {
            isOnlineMode = false;
            const onlinePanel = document.getElementById("lobby-online-panel");
            if (onlinePanel) onlinePanel.style.display = "none";
            const setupPanel = document.getElementById("lobby-setup-panel");
            if (setupPanel) setupPanel.style.display = "block";
            const startBtn = document.getElementById("btn-start-game");
            if (startBtn) startBtn.style.display = "inline-flex";
            const waitMsg = document.getElementById("lobby-waiting-msg");
            if (waitMsg) waitMsg.style.display = "none";
            initLobby();
            showScreen("lobby");
        });
    }
    
    safeAddListener("btn-create-room", "click", () => {
        if (!connection || connection.state !== "Connected") {
            showNotification("Đang kết nối tới máy chủ SignalR... Vui lòng thử lại sau vài giây.", "warning");
            initSignalR();
            return;
        }
        const playerNameInput = document.getElementById("player-name-0");
        const playerName = playerNameInput ? playerNameInput.value.trim() : "Chủ phòng";
        
        const charBtn = document.getElementById("btn-pick-char-0");
        const charName = charBtn ? charBtn.querySelector(".selected-char-name").innerText : "Lộc Phát";
        const character = CHARACTER_DATABASE.find(c => c.name === charName) || CHARACTER_DATABASE[0];

        connection.invoke("CreateRoom", playerName, character.id, sessionToken)
            .catch(err => console.error("Error creating room: ", err));
    });

    safeAddListener("btn-join-room", "click", () => {
        const modal = document.getElementById("join-room-modal");
        if (modal) {
            modal.classList.add("active");
            
            // Hide main menu content card smoothly
            const menuContent = document.querySelector(".menu-content");
            if (menuContent) {
                menuContent.style.opacity = "0";
                menuContent.style.pointerEvents = "none";
            }
            
            // Reset modal to Step 1 (Room Code)
            const stepCode = document.getElementById("join-step-code");
            const stepName = document.getElementById("join-step-name");
            if (stepCode) stepCode.style.display = "block";
            if (stepName) stepName.style.display = "none";
            
            const codeInput = document.getElementById("join-room-code");
            if (codeInput) {
                codeInput.value = "";
                setTimeout(() => codeInput.focus(), 100);
            }

            // Prefill name from local storage or default
            let savedName = "";
            try {
                const storedNames = JSON.parse(localStorage.getItem("last_player_names") || "[]");
                if (storedNames.length > 0) savedName = storedNames[0];
            } catch(e) {}
            if (!savedName) savedName = "Người chơi " + Math.floor(Math.random() * 900 + 100);
            
            const joinPlayerInput = document.getElementById("join-player-name");
            if (joinPlayerInput) joinPlayerInput.value = savedName;
        }
    });

    safeAddListener("btn-cancel-join", "click", () => {
        const modal = document.getElementById("join-room-modal");
        if (modal) modal.classList.remove("active");
        
        // Show main menu content card again
        const menuContent = document.querySelector(".menu-content");
        if (menuContent) {
            menuContent.style.opacity = "1";
            menuContent.style.pointerEvents = "auto";
        }
    });

    safeAddListener("btn-next-join", "click", () => {
        const codeInput = document.getElementById("join-room-code");
        const code = codeInput ? codeInput.value.trim().toUpperCase() : "";
        if (code.length !== 5) {
            showNotification("Mã phòng phải có đúng 5 ký tự.", "error");
            if (codeInput) codeInput.focus();
            return;
        }
        
        const stepCode = document.getElementById("join-step-code");
        const stepName = document.getElementById("join-step-name");
        if (stepCode) stepCode.style.display = "none";
        if (stepName) {
            stepName.style.display = "block";
            const nameInput = document.getElementById("join-player-name");
            if (nameInput) {
                setTimeout(() => nameInput.focus(), 100);
            }
        }
    });

    safeAddListener("btn-back-join", "click", () => {
        const stepCode = document.getElementById("join-step-code");
        const stepName = document.getElementById("join-step-name");
        if (stepCode) {
            stepCode.style.display = "block";
            const codeInput = document.getElementById("join-room-code");
            if (codeInput) {
                setTimeout(() => codeInput.focus(), 100);
            }
        }
        if (stepName) stepName.style.display = "none";
    });

    // Enter key listeners for Join modal inputs
    const joinCodeInput = document.getElementById("join-room-code");
    if (joinCodeInput) {
        joinCodeInput.addEventListener("keydown", (e) => {
            if (e.key === "Enter") {
                const nextBtn = document.getElementById("btn-next-join");
                if (nextBtn) nextBtn.click();
            }
        });
    }

    const joinNameInput = document.getElementById("join-player-name");
    if (joinNameInput) {
        joinNameInput.addEventListener("keydown", (e) => {
            if (e.key === "Enter") {
                const submitBtn = document.getElementById("btn-submit-join");
                if (submitBtn) submitBtn.click();
            }
        });
    }

    safeAddListener("btn-submit-join", "click", () => {
        if (!connection || connection.state !== "Connected") {
            showNotification("Đang kết nối tới máy chủ SignalR... Vui lòng thử lại sau vài giây.", "warning");
            initSignalR();
            return;
        }
        const codeInput = document.getElementById("join-room-code");
        const code = codeInput ? codeInput.value.trim().toUpperCase() : "";
        if (code.length !== 5) {
            showNotification("Mã phòng phải có đúng 5 ký tự.", "error");
            return;
        }
        
        const joinPlayerNameInput = document.getElementById("join-player-name");
        const playerName = joinPlayerNameInput ? joinPlayerNameInput.value.trim() : "";
        if (!playerName) {
            showNotification("Vui lòng nhập tên của bạn.", "error");
            return;
        }
        
        const character = CHARACTER_DATABASE[Math.floor(Math.random() * CHARACTER_DATABASE.length)];

        connection.invoke("JoinRoom", code, playerName, character.id, sessionToken)
            .then(() => {
                const joinModal = document.getElementById("join-room-modal");
                if (joinModal) joinModal.classList.remove("active");
                
                // Restore menu content visibility for future returns
                const menuContent = document.querySelector(".menu-content");
                if (menuContent) {
                    menuContent.style.opacity = "1";
                    menuContent.style.pointerEvents = "auto";
                }

                isOnlineMode = true;
                roomCode = code;
                localStorage.setItem("saved_room_code", code);
                
                // Save name to last player names local storage for convenience
                try {
                    let storedNames = JSON.parse(localStorage.getItem("last_player_names") || "[]");
                    storedNames = storedNames.filter(n => n !== playerName);
                    storedNames.unshift(playerName);
                    localStorage.setItem("last_player_names", JSON.stringify(storedNames.slice(0, 10)));
                } catch(e) {}

                const roomDisplay = document.getElementById("lobby-room-code-display");
                if (roomDisplay) roomDisplay.innerText = code;
                const onlinePanel = document.getElementById("lobby-online-panel");
                if (onlinePanel) onlinePanel.style.display = "block";
                const setupPanel = document.getElementById("lobby-setup-panel");
                if (setupPanel) setupPanel.style.display = "none";
                const startBtn = document.getElementById("btn-start-game");
                if (startBtn) startBtn.style.display = "none";
                const waitMsg = document.getElementById("lobby-waiting-msg");
                if (waitMsg) waitMsg.style.display = "block";
                
                // Hide host controls gear for guest
                const hostCtrl = document.getElementById("host-only-controls");
                if (hostCtrl) hostCtrl.style.display = "none";
                
                showScreen("lobby");
            })
            .catch(err => {
                console.error("Error joining room: ", err);
                showNotification(err.message || "Không thể tham gia phòng. Vui lòng kiểm tra lại mã phòng.", "error");
            });
    });

    safeAddListener("btn-question-pool", "click", () => {
        openQuestionsPool();
    });



    // Lobby Event Listeners
    safeAddListener("btn-lobby-back", "click", () => {
        if (isOnlineMode && connection) {
            showConfirm("Bạn có chắc chắn muốn rời phòng chờ?", () => {
                localStorage.removeItem("saved_room_code");
                if (connection.state === "Connected") {
                    connection.stop().then(() => {
                        isOnlineMode = false;
                        showScreen("menu");
                    }).catch(err => {
                        console.error(err);
                        isOnlineMode = false;
                        showScreen("menu");
                    });
                } else {
                    isOnlineMode = false;
                    showScreen("menu");
                }
            });
        } else {
            showScreen("menu");
        }
    });

    safeAddListener("btn-copy-lobby-code", "click", () => {
        const codeDisplay = document.getElementById("lobby-room-code-display");
        const codeText = codeDisplay ? codeDisplay.innerText : "";
        if (!codeText) return;
        navigator.clipboard.writeText(codeText).then(() => {
            const copyBtn = document.getElementById("btn-copy-lobby-code");
            if (copyBtn) {
                const originalIcon = copyBtn.innerHTML;
                copyBtn.innerHTML = `<i class="fa-solid fa-check" style="color: #10b981;"></i>`;
                setTimeout(() => {
                    copyBtn.innerHTML = originalIcon;
                }, 1500);
            }
        }).catch(err => {
            console.error("Could not copy room code: ", err);
        });
    });

    // Open Lobby Settings Modal (Gear button)
    safeAddListener("host-only-controls", "click", () => {
        const modal = document.getElementById("lobby-settings-modal");
        if (modal) modal.classList.add("active");
    });

    // Close Lobby Settings Modal
    const closeLobbySettings = () => {
        const modal = document.getElementById("lobby-settings-modal");
        if (modal) modal.classList.remove("active");
    };
    safeAddListener("btn-close-lobby-settings-modal", "click", closeLobbySettings);
    safeAddListener("lobby-settings-modal", "click", (e) => {
        if (e.target.id === "lobby-settings-modal") closeLobbySettings();
    });
    safeAddListener("btn-save-lobby-settings", "click", closeLobbySettings);

    // Lobby count selectors
    const playerCountDisplay = document.getElementById("player-count-display");
    document.querySelectorAll(".btn-count-adj").forEach(btn => {
        btn.addEventListener("click", (e) => {
            if (isOnlineMode) return;
            let current = parseInt(playerCountDisplay.innerText);
            let adj = parseInt(e.currentTarget.dataset.adj);
            let next = current + adj;
            if (next >= 2 && next <= 6) {
                playerCountDisplay.innerText = next;
                renderPlayerSetupCards(next);
            }
        });
    });

    safeAddListener("btn-start-game", "click", () => {
        if (isOnlineMode) {
            const specChk = document.getElementById("chk-host-spectate");
            const isSpectator = specChk ? specChk.checked : false;
            const durInput = document.getElementById("input-game-duration");
            const durationVal = durInput ? parseInt(durInput.value) : 10;
            const durationMinutes = isNaN(durationVal) || durationVal <= 0 ? 10 : durationVal;
            connection.invoke("StartGame", roomCode, isSpectator, durationMinutes)
                .catch(err => console.error("Error starting game: ", err));
        } else {
            startGame();
        }
    });

    // 5. Gameplay Event Listeners
    safeAddListener("btn-roll-dice", "click", handleRollDice);
    safeAddListener("btn-quit-game", "click", () => {
        showConfirm("Bạn có chắc muốn thoát ván chơi này? Tiến trình chơi sẽ bị mất.", () => {
            if (isOnlineMode && connection) {
                localStorage.removeItem("saved_room_code");
                window.location.reload();
            } else {
                showScreen("menu");
            }
        });
    });

    // Close Modals
    safeAddListener("btn-close-settings", "click", () => {
        const modal = document.getElementById("settings-modal");
        if (modal) modal.classList.remove("active");
    });
    
    safeAddListener("btn-save-settings", "click", () => {
        saveSettings();
        const modal = document.getElementById("settings-modal");
        if (modal) modal.classList.remove("active");
    });

    safeAddListener("btn-close-qpool", "click", () => {
        const modal = document.getElementById("questions-pool-modal");
        if (modal) modal.classList.remove("active");
    });

    safeAddListener("btn-close-trap-modal", "click", () => {
        const modal = document.getElementById("trap-modal");
        if (modal) modal.classList.remove("active");
        if (!isOnlineMode) nextTurn();
    });

    safeAddListener("btn-close-reward-modal", "click", () => {
        const modal = document.getElementById("reward-modal");
        if (modal) modal.classList.remove("active");
        if (!isOnlineMode) nextTurn();
    });

    safeAddListener("btn-close-wheel-modal", "click", () => {
        const modal = document.getElementById("wheel-modal");
        if (modal) modal.classList.remove("active");
        if (!isOnlineMode) nextTurn();
    });

    // 6. Victory Event Listeners
    safeAddListener("btn-play-again", "click", () => {
        resetGameStates();
        showScreen("lobby");
    });

    safeAddListener("btn-victory-home", "click", () => {
        showScreen("menu");
    });

    // Render Lobby initially
    renderPlayerSetupCards(4);
}

// Initialise SignalR right away on start
setTimeout(() => {
    initSignalR();
}, 2500);
