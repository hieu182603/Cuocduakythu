// ==================================================
// SIGNALR MULTIPLAYER INTEGRATION & BROADCASTS
// ==================================================

let signalRRestartTimer = null;
const pendingRemotePlayerUpdates = new Map();
let remotePlayerFrame = 0;

function setGameplayConnectionState(isConnected) {
    const rollButton = document.getElementById("btn-roll-dice");
    if (rollButton && isOnlineMode && myPlayerId !== -1) rollButton.disabled = !isConnected;
    document.body.classList.toggle("signalr-disconnected", !isConnected);
}

function startSignalRConnection() {
    if (!connection || connection.state !== "Disconnected") return;
    connection.start()
        .then(() => {
            setGameplayConnectionState(true);
            const savedRoomCode = localStorage.getItem("saved_room_code");
            if (savedRoomCode && sessionToken) {
                return connection.invoke("RejoinRoom", savedRoomCode, sessionToken);
            }
        })
        .catch(error => {
            console.warn("SignalR connection failed; retrying.", error);
            if (!signalRRestartTimer) {
                signalRRestartTimer = setTimeout(() => {
                    signalRRestartTimer = null;
                    startSignalRConnection();
                }, 2000 + Math.random() * 3000);
            }
        });
}

function queueRemotePlayerUpdate(playerId, targetTileIndex, lapCount) {
    pendingRemotePlayerUpdates.set(playerId, { targetTileIndex, lapCount });
    if (remotePlayerFrame) return;
    remotePlayerFrame = requestAnimationFrame(() => {
        remotePlayerFrame = 0;
        pendingRemotePlayerUpdates.forEach((update, id) => {
            const player = players.find(item => item.id === id);
            if (player) {
                player.tileIndex = update.targetTileIndex;
                player.lapCount = update.lapCount;
            }
        });
        pendingRemotePlayerUpdates.clear();
        updatePlayerPositionsOnBoard();
        renderScoreboard();
    });
}

function initSignalR() {
    if (typeof signalR === "undefined") {
        console.warn("SignalR CDN is not loaded yet.");
        return;
    }
    if (connection) {
        if (connection.state === "Disconnected") {
            startSignalRConnection();
        }
        return;
    }

    connection = new signalR.HubConnectionBuilder()
        .withUrl(APP_CONFIG.SIGNALR_URL)
        .withAutomaticReconnect({
            nextRetryDelayInMilliseconds: retryContext => {
                const base = Math.min(30000, 1000 * Math.pow(2, retryContext.previousRetryCount));
                return Math.min(30000, base + Math.random() * 1000);
            }
        })
        .build();
    connection.serverTimeoutInMilliseconds = 60000;
    connection.keepAliveIntervalInMilliseconds = 15000;

    connection.on("Error", (message) => {
        showNotification(message, "error");
    });

    connection.on("RoomCreated", (code, playersList) => {
        isOnlineMode = true;
        roomCode = code;
        localStorage.setItem("saved_room_code", code);
        document.getElementById("lobby-room-code-display").innerText = code;
        document.getElementById("lobby-online-panel").style.display = "block";
        document.getElementById("lobby-setup-panel").style.display = "none";
        
        // Determine host status dynamically
        const me = playersList.find(p => p.connectionId === connection.connectionId);
        isHost = me ? me.isHost : false;
        
        if (me) {
            myPlayerId = me.isSpectator ? -1 : me.id;
        } else {
            myPlayerId = 0;
        }

        const startBtn = document.getElementById("btn-start-game");
        const waitMsg = document.getElementById("lobby-waiting-msg");
        const hostCtrl = document.getElementById("host-only-controls");

        if (isHost) {
            if (startBtn) startBtn.style.display = "inline-flex";
            if (waitMsg) waitMsg.style.display = "none";
            if (hostCtrl) hostCtrl.style.display = "inline-flex";
        } else {
            if (startBtn) startBtn.style.display = "none";
            if (waitMsg) waitMsg.style.display = "block";
            if (hostCtrl) hostCtrl.style.display = "none";
        }
        
        syncLobbyPlayers(playersList);
        showScreen("lobby");
        console.log("Joined Room: " + code + ", isHost: " + isHost);
    });

    connection.on("PlayerJoined", (playersList) => {
        syncLobbyPlayers(playersList);
        console.log("Player joined the lobby.");
    });

    connection.on("GameStarted", (playersList, activeIdx, durationMinutes, gameEndTimeUtc) => {
        isGameEnding = false;
        players = playersList.map(p => {
            const char = CHARACTER_DATABASE.find(c => c.id === p.horseId) || CHARACTER_DATABASE[p.id % CHARACTER_DATABASE.length];
            return {
                id: p.id,
                connectionId: p.connectionId,
                name: p.name,
                character: char,
                tileIndex: p.tileIndex,
                wrongStreak: p.wrongStreak,
                shield: p.shield,
                freezeTimeMs: p.freezeTimeMs,
                doubleDice: p.doubleDice,
                isAutoRoll: p.isAutoRoll,
                diceModifier: p.diceModifier,
                lapCount: p.lapCount,
                isSpectator: p.isSpectator
            };
        });

        activePlayerIndex = activeIdx;

        const me = players.find(p => p.connectionId === connection.connectionId);
        if (me) myPlayerId = me.isSpectator ? -1 : me.id;

        renderBoard();
        updatePlayerPositionsOnBoard();
        renderScoreboard();
        
        if (durationMinutes) {
            startGameTimer(durationMinutes, gameEndTimeUtc);
        } else {
            const timerCard = document.getElementById("game-timer-card");
            if (timerCard) timerCard.style.display = "none";
        }

        // Leaderboard visibility during gameplay
        const showLeaderboard = true;
        const sbHeader = document.querySelector(".gameplay-sidebar .sidebar-header");
        if (sbHeader) sbHeader.style.display = showLeaderboard ? "block" : "none";
        const sbList = document.getElementById("scoreboard-list");
        if (sbList) sbList.style.display = showLeaderboard ? "flex" : "none";

        // Setup local racer roll button
        const btnRollDice = document.getElementById("btn-roll-dice");
        const centerInfo = document.getElementById("center-interactive-info");
        if (myPlayerId !== -1) {
            if (btnRollDice) btnRollDice.disabled = false;
            if (centerInfo) centerInfo.innerText = "Hãy tung xúc xắc để di chuyển!";
        } else {
            if (btnRollDice) btnRollDice.disabled = true;
            if (centerInfo) centerInfo.innerText = "Bạn đang ở chế độ xem trận đấu.";
        }

        // Reset visual dice values
        const visualDice1 = document.getElementById("visual-dice-1");
        if (visualDice1) {
            visualDice1.style.transform = "rotateX(0deg) rotateY(0deg) rotateZ(0deg)";
            visualDice1.classList.remove("dice-shake");
        }
    

        showScreen("gameplay");
        logMessage("Cuộc đua online đã bắt đầu! Đang ở vạch xuất phát.", "log-win");
    });

    connection.on("DiceRolled", (playerName, rollVal1, rollVal2, totalMove) => {
        const dice1 = document.getElementById("visual-dice-1");
        roll3DDice(dice1, rollVal1);
        
        setTimeout(() => {
            const isDouble = (totalMove === rollVal1 * 2) && (totalMove !== rollVal1);
            logMessage(`[${playerName}] xúc xắc được ${rollVal1} (Di chuyển: ${totalMove} ô).`);
        }, 1500);
    });

    connection.on("PlayerMoved", (playerId, targetTileIndex, lapCount, direction, steps) => {
        if (playerId === myPlayerId) {
            queueOnlineMovement({ playerId, targetTileIndex, lapCompleted: lapCount, direction, steps });
        } else {
            // Do not let animations from dozens of remote players block this
            // client's personal dice/event queue.
            queueRemotePlayerUpdate(playerId, targetTileIndex, lapCount);
        }
    });

    connection.on("EnableRollDice", () => {
        queueOnlineCallback(() => {
            if (typeof window.checkPostPopupState === "function") {
                // This event is emitted for a normal landing/start fallback,
                // not as acknowledgement of a result modal.
                window.checkPostPopupState(true);
            } else {
                document.getElementById("btn-roll-dice").disabled = false;
            }
        });
    });

    connection.on("TriggerTrap", (playerId, playerName) => {
        if (myPlayerId !== -1 && playerId === myPlayerId) {
            connection.invoke("ResolveTrap", roomCode);
        }
    });

    connection.on("TriggerReward", (playerId, playerName) => {
        if (myPlayerId !== -1 && playerId === myPlayerId) {
            connection.invoke("ResolveReward", roomCode);
        }
    });

    connection.on("TriggerQuestion", (playerId, playerName, questionText, answersList, wrongStreak) => {
        queueOnlineCallback(() => new Promise(resolvePopup => {
            const player = players.find(p => p.id === playerId);
            currentQuestion = { question: questionText, answers: answersList, correct: -1 };
            
            const modal = document.getElementById("question-modal");
            document.getElementById("question-text").innerText = questionText;
            
            const streakWarn = document.getElementById("wrong-streak-warning");
            if (wrongStreak > 0) {
                streakWarn.innerText = `⚠️ CHUỖI SAI: ${wrongStreak} lần!`;
                streakWarn.style.display = "block";
            } else {
                streakWarn.style.display = "none";
            }

            const answersGrid = document.getElementById("answers-grid");
            answersGrid.innerHTML = "";

            const isActive = myPlayerId !== -1;

            answersList.forEach((ans, idx) => {
                const btn = document.createElement("button");
                btn.className = "answer-btn";
                btn.textContent = `${String.fromCharCode(65 + idx)}. ${ans}`;
                
                if (isActive) {
                    btn.addEventListener("click", () => {
                        if (btn.disabled) return;
                        const allBtns = document.querySelectorAll(".answer-btn");
                        allBtns.forEach(b => b.classList.remove("active"));
                        btn.classList.add("active");
                        
                        stopQuestionTimer();
                        connection.invoke("SubmitAnswer", roomCode, idx);
                    });
                } else {
                    btn.disabled = true;
                    btn.style.cursor = "not-allowed";
                }
                answersGrid.appendChild(btn);
            });

            modal.classList.add("active");

            startQuestionTimer(15, () => {
                if (isActive) {
                    connection.invoke("SubmitAnswer", roomCode, -1);
                }
            });

            window.resolveQuestionPopup = resolvePopup;
        }));
    });

    connection.on("AnswerOutcome", (playerId, playerName, isCorrect, correctIndex, wrongStreak, penaltyText, canAcknowledge) => {
        stopQuestionTimer();
        const allButtons = document.querySelectorAll(".answer-btn");
        allButtons.forEach(btn => btn.disabled = true);

        // Highlight correct/incorrect answers
        if (correctIndex >= 0 && correctIndex < allButtons.length) {
            allButtons[correctIndex].classList.add("correct");
        }
        
        // Find and highlight active button (clicked by local user)
        const activeBtn = document.querySelector(".answer-btn.active");
        if (activeBtn && !isCorrect) {
            activeBtn.classList.add("incorrect");
        }

        playSFX(isCorrect ? 'success' : 'fail');

        if (isCorrect) {
            logMessage(`[${playerName}] trả lời ĐÚNG!`, "log-question");
        } else {
            logMessage(`[${playerName}] trả lời SAI! ${penaltyText ? "Hình phạt: " + penaltyText : ""}`, "log-trap");
        }

        setTimeout(() => {
            document.getElementById("question-modal").classList.remove("active");
            if (myPlayerId !== -1) {
                if (canAcknowledge && typeof checkPostPopupState === "function") checkPostPopupState(); else if (!canAcknowledge) document.getElementById("btn-roll-dice").disabled = true;
                document.getElementById("center-interactive-info").innerText = "Hãy tiếp tục tung xúc xắc!";
            }
            // Resume the queue if the QuestionTriggered callback is waiting
            if (typeof window.resolveQuestionPopup === "function") {
                window.resolveQuestionPopup();
                window.resolveQuestionPopup = null;
            }
        }, 2000);
    });

    connection.on("TrapResolved", (playerId, playerName, trapName, trapDetail, newTileIndex, freezeTimeMs, landingTileIndex) => {
        queueOnlineCallback(() => new Promise(resolvePopup => {
            const player = players.find(p => p.id === playerId);
            const modal = document.getElementById("trap-modal");
            const cardsContainer = document.getElementById("trap-cards-container");
            const closeBtn = document.getElementById("btn-close-trap-modal");
            const isActive = myPlayerId !== -1;
            const eventTileIndex = Number.isInteger(landingTileIndex)
                ? landingTileIndex
                : (player ? player.tileIndex : -1);



            // Reset elements
            document.getElementById("trap-result-box").style.display = "none";
            closeBtn.style.display = "none";
            cardsContainer.style.display = "flex";
            cardsContainer.innerHTML = "";

            if (isActive) {
                document.getElementById("trap-modal-desc").innerText = "Ôi không! Bạn đã dẫm phải bẫy hiểm họa. Hãy chọn 1 lá bài!";
            } else {
                document.getElementById("trap-modal-desc").innerText = `[${playerName}] đã dẫm phải bẫy hiểm họa và đang chọn bài...`;
            }

            // We need 2 alternative traps for display
            let alternatives = TRAP_LIST.filter(t => t.name !== trapName);
            alternatives.sort(() => Math.random() - 0.5);
            let cardChoices = [{ name: trapName, detail: trapDetail }, alternatives[0], alternatives[1]];
            cardChoices.sort(() => Math.random() - 0.5);

            let hasSelected = false;

            cardChoices.forEach((choice, index) => {
                const card = document.createElement("div");
                card.className = "event-card";
                card.innerHTML = `
                    <div class="card-back">
                        <i class="fa-solid fa-skull-crossbones"></i>
                        <span>Bẫy ${index + 1}</span>
                    </div>
                    <div class="card-front">
                        <div class="card-front-icon"><i class="fa-solid fa-triangle-exclamation"></i></div>
                        <div class="effect-name">${escapeHTML(choice.name)}</div>
                        <div class="effect-detail">${escapeHTML(choice.detail)}</div>
                    </div>
                `;

                if (isActive) {
                    card.addEventListener("click", () => {
                        if (hasSelected) return;
                        hasSelected = true;
                        playSFX('fail');

                        // Flip and reveal
                        const cardInnerElements = cardsContainer.querySelectorAll(".event-card");
                        cardInnerElements.forEach((c, idx) => {
                            c.classList.add("disabled");
                            const frontName = c.querySelector(".effect-name");
                            const frontDetail = c.querySelector(".effect-detail");
                            if (idx === index) {
                                frontName.innerText = trapName;
                                frontDetail.innerText = trapDetail;
                                c.classList.add("flipped");
                            } else {
                                let altIdx = idx < index ? idx : idx - 1;
                                frontName.innerText = alternatives[altIdx].name;
                                frontDetail.innerText = alternatives[altIdx].detail;
                                c.classList.add("flipped");
                                c.classList.add("faded");
                            }
                        });

                        // Apply and log
                        if (player) {
                            player.freezeTimeMs = freezeTimeMs;
                        }
                        renderScoreboard();
                        logMessage(`[${playerName}] kích hoạt bẫy: ${trapName} - ${trapDetail}`, "log-trap");

                        setTimeout(() => {
                            document.getElementById("trap-modal-desc").innerText = "Chi tiết bẫy kích hoạt:";
                            document.getElementById("trap-result-box").style.display = "block";
                            document.getElementById("trap-result-box").innerHTML = `
                                <div class="effect-name">${escapeHTML(trapName)}</div>
                                <div class="effect-detail">${escapeHTML(trapDetail)}</div>
                            `;
                            closeBtn.style.display = "inline-flex";
                            closeBtn.onclick = () => {
                                modal.classList.remove("active");
                                resolvePopup();
                                if (myPlayerId !== -1) {
                                    const tile = TILE_COORDS[newTileIndex];
                                    // Only process new tile landing if they actually moved to a different non-start tile
                                    if (newTileIndex !== eventTileIndex && tile.type !== "start") {
                                        connection.invoke("ProcessNewTileLanding", roomCode).catch(err => console.warn("Landing already processed:", err));
                                    } else {
                                        if (typeof checkPostPopupState === "function") checkPostPopupState(); else document.getElementById("btn-roll-dice").disabled = false;
                                        document.getElementById("center-interactive-info").innerText = "Hãy tiếp tục tung xúc xắc!";
                                    }
                                }
                            };
                        }, 800);
                    });
                }

                cardsContainer.appendChild(card);
            });

            modal.classList.add("active");
        }));
    });

    connection.on("RewardResolved", (playerId, playerName, rewardName, rewardDetail, newTileIndex, shield, doubleDice, isExtraTurn) => {
        queueOnlineCallback(() => new Promise(resolvePopup => {
            const player = players.find(p => p.id === playerId);
            const modal = document.getElementById("reward-modal");
            const cardsContainer = document.getElementById("reward-cards-container");
            const closeBtn = document.getElementById("btn-close-reward-modal");
            const isActive = myPlayerId !== -1;
            const eventTileIndex = player ? player.tileIndex : -1;

            // Reset elements
            document.getElementById("reward-result-box").style.display = "none";
            closeBtn.style.display = "none";
            cardsContainer.style.display = "flex";
            cardsContainer.innerHTML = "";

            if (isActive) {
                document.getElementById("reward-modal-desc").innerText = "Tuyệt vời! Bạn nhận được một phần quà may mắn. Hãy chọn 1 lá bài!";
            } else {
                document.getElementById("reward-modal-desc").innerText = `[${playerName}] nhận được phần quà may mắn và đang chọn bài...`;
            }

            // We need 2 alternative rewards for display
            let alternatives = REWARD_LIST.filter(r => r.name !== rewardName);
            alternatives.sort(() => Math.random() - 0.5);
            let cardChoices = [{ name: rewardName, detail: rewardDetail }, alternatives[0], alternatives[1]];
            cardChoices.sort(() => Math.random() - 0.5);

            let hasSelected = false;

            cardChoices.forEach((choice, index) => {
                const card = document.createElement("div");
                card.className = "event-card";
                card.innerHTML = `
                    <div class="card-back">
                        <i class="fa-solid fa-gift"></i>
                        <span>Thưởng ${index + 1}</span>
                    </div>
                    <div class="card-front">
                        <div class="card-front-icon"><i class="fa-solid fa-circle-check"></i></div>
                        <div class="effect-name">${escapeHTML(choice.name)}</div>
                        <div class="effect-detail">${escapeHTML(choice.detail)}</div>
                    </div>
                `;

                if (isActive) {
                    card.addEventListener("click", () => {
                        if (hasSelected) return;
                        hasSelected = true;
                        playSFX('success');

                        // Flip and reveal
                        const cardInnerElements = cardsContainer.querySelectorAll(".event-card");
                        cardInnerElements.forEach((c, idx) => {
                            c.classList.add("disabled");
                            const frontName = c.querySelector(".effect-name");
                            const frontDetail = c.querySelector(".effect-detail");
                            if (idx === index) {
                                frontName.innerText = rewardName;
                                frontDetail.innerText = rewardDetail;
                                c.classList.add("flipped");
                            } else {
                                let altIdx = idx < index ? idx : idx - 1;
                                frontName.innerText = alternatives[altIdx].name;
                                frontDetail.innerText = alternatives[altIdx].detail;
                                c.classList.add("flipped");
                                c.classList.add("faded");
                            }
                        });

                        // Apply and log
                        if (player) {
                            player.shield = shield;
                            player.doubleDice = doubleDice;
                            player.isAutoRoll = isExtraTurn;
                        }
                        renderScoreboard();
                        logMessage(`[${playerName}] kích hoạt thưởng: ${rewardName} - ${rewardDetail}`, "log-reward");

                        setTimeout(() => {
                            document.getElementById("reward-modal-desc").innerText = "Chi tiết phần thưởng:";
                            document.getElementById("reward-result-box").style.display = "block";
                            document.getElementById("reward-result-box").innerHTML = `
                                <div class="effect-name">${escapeHTML(rewardName)}</div>
                                <div class="effect-detail">${escapeHTML(rewardDetail)}</div>
                            `;
                            closeBtn.style.display = "inline-flex";
                            closeBtn.onclick = () => {
                                modal.classList.remove("active");
                                resolvePopup();
                                if (myPlayerId !== -1) {
                                    const tile = TILE_COORDS[newTileIndex];
                                    // Only process new tile landing if they actually moved to a different non-start tile
                                    if (newTileIndex !== eventTileIndex && tile.type !== "start") {
                                        connection.invoke("ProcessNewTileLanding", roomCode).catch(err => console.warn("Landing already processed:", err));
                                    } else {
                                        if (typeof checkPostPopupState === "function") checkPostPopupState(); else document.getElementById("btn-roll-dice").disabled = false;
                                        document.getElementById("center-interactive-info").innerText = "Hãy tiếp tục tung xúc xắc!";
                                    }
                                }
                            };
                        }, 800);
                    });
                }

                cardsContainer.appendChild(card);
            });

            modal.classList.add("active");
        }));
    });

    connection.on("TriggerShieldBlock", (playerId, playerName) => {
        logMessage(`Lá chắn của [${playerName}] đã hóa giải bẫy thành công!`, "log-reward");
        const modal = document.getElementById("reward-modal");
        
        document.getElementById("reward-modal-desc").innerText = "Lá chắn kích hoạt!";
        document.getElementById("reward-cards-container").style.display = "none";
        document.getElementById("reward-result-box").style.display = "block";
        
        document.getElementById("reward-result-box").innerHTML = `
            <div class="effect-name" style="color:var(--success);">KÍCH HOẠT LÁ CHẮN!</div>
            <div class="effect-detail">Lá chắn của ${escapeHTML(playerName)} đã hóa giải hoàn toàn bẫy hiểm họa!</div>
        `;
        modal.classList.add("active");

        const isActive = myPlayerId !== -1;
        const closeBtn = document.getElementById("btn-close-reward-modal");
        if (isActive) {
            closeBtn.style.display = "inline-flex";
            closeBtn.onclick = () => {
                modal.classList.remove("active");
                if (myPlayerId !== -1) {
                    if (typeof checkPostPopupState === "function") checkPostPopupState(); else document.getElementById("btn-roll-dice").disabled = false;
                    document.getElementById("center-interactive-info").innerText = "Hãy tiếp tục tung xúc xắc!";
                }
            };
        } else {
            closeBtn.style.display = "none";
        }
    });

    connection.on("TriggerWheel", (playerId, playerName) => {
        queueOnlineCallback(() => new Promise(resolvePopup => {
            const modal = document.getElementById("wheel-modal");
            document.getElementById("wheel-result-text").style.display = "none";
            document.getElementById("btn-spin-wheel").style.display = "none";
            document.getElementById("btn-close-wheel-modal").style.display = "none";
            
            drawWheel(0);
            modal.classList.add("active");

            const isActive = myPlayerId !== -1;
            if (isActive) {
                document.getElementById("btn-spin-wheel").style.display = "inline-flex";
                document.getElementById("btn-spin-wheel").disabled = false;
                document.getElementById("btn-spin-wheel").onclick = () => {
                    connection.invoke("SpinWheel", roomCode);
                };
            }
            
            // To resolve the popup, store it so WheelSpun can resume the queue
            window.resolveWheelPopup = resolvePopup;
        }));
    });

    connection.on("WheelSpun", (playerId, playerName, sectorIndex, label, desc, isReward, newTileIndex, freezeTimeMs, shield, isAutoRoll) => {
        const player = players.find(p => p.id === playerId);
        let eventTileIndex = -1;
        if (player) {
            eventTileIndex = player.tileIndex;
            player.freezeTimeMs = freezeTimeMs;
            player.shield = shield;
            player.isAutoRoll = isAutoRoll;
            if (sectorIndex === 6) {
                player.doubleDice = true;
            }
        }

        const numSectors = WHEEL_SECTORS.length;
        const arc = Math.PI * 2 / numSectors;
        
        let targetAngle = (Math.PI * 3.5) - (sectorIndex * arc) - (arc / 2);
        let finalSpinAngle = targetAngle + Math.PI * 8; // 4 spins
        
        document.getElementById("btn-spin-wheel").style.display = "none";
        
        let start = null;
        const duration = 3000;
        
        isWheelSpinning = true;
        playSFX('click');

        function animateSpin(timestamp) {
            if (!start) start = timestamp;
            let progress = timestamp - start;
            let t = progress / duration;
            t = (--t) * t * t + 1; // ease out cubic
            
            let angle = t * finalSpinAngle;
            drawWheel(angle);
            
            if (progress < duration) {
                requestAnimationFrame(animateSpin);
            } else {
                isWheelSpinning = false;
                playSFX(isReward ? 'success' : 'fail');
                
                let resultText = document.getElementById("wheel-result-text");
                resultText.textContent = `${label}\n${desc}`;
                resultText.style.whiteSpace = "pre-line";
                resultText.style.display = "block";
                
                updatePlayerPositionsOnBoard();
                renderScoreboard();
                logMessage(`[${playerName}] quay vòng quay trúng: ${label} (${desc})`);

                setTimeout(() => {
                    document.getElementById("wheel-modal").classList.remove("active");
                    if (typeof window.resolveWheelPopup === "function") {
                        window.resolveWheelPopup();
                        window.resolveWheelPopup = null;
                    }
                    if (myPlayerId !== -1) {
                        const tile = TILE_COORDS[newTileIndex];
                        if (newTileIndex !== eventTileIndex && tile.type !== "start") {
                            connection.invoke("ProcessNewTileLanding", roomCode).catch(err => console.warn("Landing already processed:", err));
                        } else {
                            if (typeof checkPostPopupState === "function") checkPostPopupState(); else document.getElementById("btn-roll-dice").disabled = false;
                            document.getElementById("center-interactive-info").innerText = "Hãy tiếp tục tung xúc xắc!";
                        }
                    }
                }, 2000);
            }
        }
        
        requestAnimationFrame(animateSpin);
    });

    connection.on("NextTurnTriggered", (nextIdx) => {
        // Obsolete in simultaneous mode
    });

    connection.on("StatusUpdate", (msg, className) => {
        logMessage(msg, className);
    });

    connection.on("GameFinished", (winner, finalRanking) => {
        isGameEnding = true;
        if (Array.isArray(finalRanking) && finalRanking.length > 0) {
            players = finalRanking.map(p => ({
                id: p.id,
                connectionId: p.connectionId,
                name: p.name,
                horseId: p.horseId,
                character: CHARACTER_DATABASE.find(c => c.id === p.horseId) || CHARACTER_DATABASE[0],
                tileIndex: p.tileIndex,
                wrongStreak: p.wrongStreak,
                shield: p.shield,
                freezeTimeMs: p.freezeTimeMs,
                doubleDice: p.doubleDice,
                isAutoRoll: p.isAutoRoll,
                diceModifier: p.diceModifier,
                lapCount: p.lapCount,
                isSpectator: p.isSpectator
            }));
        }
        
        const localWinner = players.find(p => p.id === winner.id) || winner;
        if (!localWinner.character) {
            localWinner.character = CHARACTER_DATABASE.find(c => c.id === localWinner.horseId) || CHARACTER_DATABASE[0];
        }
        triggerVictory(localWinner);
    });

    connection.on("GameReset", playersList => {
        isGameEnding = false;
        resetGameStates();
        isOnlineMode = true;
        players = playersList.map(p => ({
            id: p.id,
            connectionId: p.connectionId,
            name: p.name,
            character: CHARACTER_DATABASE.find(c => c.id === p.horseId) || CHARACTER_DATABASE[0],
            tileIndex: 0,
            wrongStreak: 0,
            shield: false,
            freezeTimeMs: 0,
            doubleDice: false,
            isAutoRoll: false,
            diceModifier: 0,
            lapCount: 0,
            isSpectator: false
        }));
        const me = players.find(p => p.connectionId === connection.connectionId);
        myPlayerId = me?.isSpectator ? -1 : (me?.id ?? -1);
        isHost = Boolean(me?.isHost);
        syncLobbyPlayers(playersList);
        showScreen("lobby");
        logMessage(isHost ? "Phòng đã sẵn sàng. Bạn có thể bắt đầu lại." : "Chủ phòng đang chuẩn bị chơi lại.", "log-reward");
    });

    connection.on("PlayerDisconnected", (playerName, playersList) => {
        logMessage(`[Hệ thống] Tay đua [${playerName}] đã ngắt kết nối.`, "log-trap");
        if (screens.lobby.classList.contains("active")) {
            syncLobbyPlayers(playersList);
        } else {
            players = playersList.map(p => {
                const char = CHARACTER_DATABASE.find(c => c.id === p.horseId) || CHARACTER_DATABASE[p.id % CHARACTER_DATABASE.length];
                return {
                    id: p.id,
                    connectionId: p.connectionId,
                    name: p.name,
                    character: char,
                    tileIndex: p.tileIndex,
                    wrongStreak: p.wrongStreak,
                    shield: p.shield,
                    skipTurn: p.skipTurn,
                    doubleDice: p.doubleDice,
                    diceModifier: p.diceModifier,
                    lapCount: p.lapCount,
                    isSpectator: p.isSpectator
                };
            });
            renderScoreboard();
            updatePlayerPositionsOnBoard();
        }
    });

    connection.on("Error", (msg) => {
        const lobbyActive = screens.lobby && screens.lobby.classList.contains("active");
        const gameplayActive = screens.gameplay && screens.gameplay.classList.contains("active");
        if (!lobbyActive && !gameplayActive && (msg.includes("không tồn tại") || msg.includes("đã bị xóa") || msg.includes("Không tìm thấy"))) {
            localStorage.removeItem("saved_room_code");
            return;
        }
        showNotification("Lỗi máy chủ: " + msg, "error");
    });

    connection.on("Rejoined", (roomState) => {
        // A late reconnect response from a room that has just finished must not
        // overwrite the final leaderboard with the lobby screen.
        if (!roomState.isStarted && (isGameEnding || screens.victory?.classList.contains("active"))) {
            return;
        }

        isOnlineMode = true;
        roomCode = roomState.roomCode;
        
        // Determine if I am the host
        const me = roomState.players.find(p => p.connectionId === connection.connectionId);
        if (me) {
            myPlayerId = me.isSpectator ? -1 : me.id;
            isHost = me.isHost;
        }

        // Sync players
        players = roomState.players.map(p => {
            const char = CHARACTER_DATABASE.find(c => c.id === p.horseId) || CHARACTER_DATABASE[p.id % CHARACTER_DATABASE.length];
            return {
                id: p.id,
                connectionId: p.connectionId,
                name: p.name,
                character: char,
                tileIndex: p.tileIndex,
                wrongStreak: p.wrongStreak,
                shield: p.shield,
                skipTurn: p.skipTurn,
                doubleDice: p.doubleDice,
                diceModifier: p.diceModifier,
                lapCount: p.lapCount,
                isSpectator: p.isSpectator
            };
        });

        // A refreshed/reconnected client may have missed the original
        // GameFinished broadcast. Rebuild the same final result from room state.
        if (roomState.isFinished) {
            const finalWinner = players
                .filter(p => !p.isSpectator)
                .sort((a, b) => (b.lapCount - a.lapCount) || (b.tileIndex - a.tileIndex))[0];
            isGameEnding = true;
            if (finalWinner) triggerVictory(finalWinner);
            else showScreen("victory");
            return;
        }

        if (roomState.isStarted) {
            // Re-render board and gameplay
            renderBoard();
            updatePlayerPositionsOnBoard();
            renderScoreboard();
            
            // Enable/disable roll button
            const btnRollDice = document.getElementById("btn-roll-dice");
            const centerInfo = document.getElementById("center-interactive-info");
            if (myPlayerId !== -1) {
                if (btnRollDice) btnRollDice.disabled = false;
                if (centerInfo) centerInfo.innerText = "Bạn đã kết nối lại. Hãy tiếp tục chơi!";
            } else {
                if (btnRollDice) btnRollDice.disabled = true;
                if (centerInfo) centerInfo.innerText = "Bạn đang ở chế độ xem trận đấu.";
            }
            
            if (roomState.gameDurationMinutes || roomState.gameEndTimeUtc) {
                startGameTimer(roomState.gameDurationMinutes, roomState.gameEndTimeUtc);
            }
            
            // Ensure host controls are visible/hidden
            const hostControls = document.getElementById("host-only-controls");
            if (hostControls) hostControls.style.display = isHost ? "inline-flex" : "none";
            
            if (isHost) {
                const btnStartGame = document.getElementById("btn-start-game");
                if (btnStartGame) btnStartGame.style.display = "none";
                
                const lobbyWaitingMsg = document.getElementById("lobby-waiting-msg");
                if (lobbyWaitingMsg) lobbyWaitingMsg.style.display = "none";
            }

            showScreen("gameplay");
            logMessage("Đã kết nối lại thành công vào trận đấu!", "log-reward");
        } else {
            // In lobby
            document.getElementById("lobby-room-code-display").innerText = roomCode;
            document.getElementById("lobby-online-panel").style.display = "block";
            document.getElementById("lobby-setup-panel").style.display = "none";
            
            if (isHost) {
                document.getElementById("btn-start-game").style.display = "inline-flex";
                document.getElementById("lobby-waiting-msg").style.display = "none";
                document.getElementById("host-only-controls").style.display = "inline-flex";
            } else {
                document.getElementById("btn-start-game").style.display = "none";
                document.getElementById("lobby-waiting-msg").style.display = "block";
                document.getElementById("host-only-controls").style.display = "none";
            }
            
            syncLobbyPlayers(roomState.players);
            showScreen("lobby");
            logMessage("Đã kết nối lại thành công vào phòng chờ!", "log-reward");
        }
    });

    connection.onreconnected((connectionId) => {
        console.log("SignalR reconnected. ConnectionId: " + connectionId);
        setGameplayConnectionState(true);
        if (isOnlineMode && roomCode && sessionToken && !isGameEnding) {
            connection.invoke("RejoinRoom", roomCode, sessionToken)
                .catch(err => console.error("Error rejoining room: ", err));
        }
    });

    connection.onreconnecting(error => {
        setGameplayConnectionState(false);
        logMessage("Mất kết nối máy chủ. Đang thử kết nối lại...", "log-trap");
        console.warn("SignalR reconnecting", error);
    });

    connection.onclose(error => {
        setGameplayConnectionState(false);
        console.warn("SignalR closed", error);
        startSignalRConnection();
    });

    startSignalRConnection();
}

function syncLobbyPlayers(playersList) {
    const listContainer = document.getElementById("players-setup-list");
    if (!listContainer) return;
    
    document.getElementById("player-count-display").innerText = playersList.length;

    const onlineCountEl = document.getElementById("online-player-count");
    if (onlineCountEl) {
        onlineCountEl.innerText = `${playersList.length}/50`;
    }

    // Determine host status dynamically based on playersList
    const me = playersList.find(p => p.connectionId === connection.connectionId);
    isHost = me ? me.isHost : false;

    const startBtn = document.getElementById("btn-start-game");
    const waitMsg = document.getElementById("lobby-waiting-msg");
    const hostCtrl = document.getElementById("host-only-controls");

    if (isHost) {
        if (startBtn) startBtn.style.display = "inline-flex";
        if (waitMsg) waitMsg.style.display = "none";
        if (hostCtrl) hostCtrl.style.display = "inline-flex";
    } else {
        if (startBtn) startBtn.style.display = "none";
        if (waitMsg) waitMsg.style.display = "block";
        if (hostCtrl) hostCtrl.style.display = "none";
    }

    // Remove extra cards if players count decreased
    while (listContainer.children.length > playersList.length) {
        listContainer.removeChild(listContainer.lastChild);
    }

    playersList.forEach((p, idx) => {
        const character = CHARACTER_DATABASE.find(c => c.id === p.horseId) || CHARACTER_DATABASE[idx % CHARACTER_DATABASE.length];
        const cardId = `player-setup-card-${idx}`;
        let card = document.getElementById(cardId);
        const isMe = p.connectionId === connection.connectionId;

        if (!card) {
            card = document.createElement("div");
            card.className = "player-setup-card";
            card.id = cardId;
            listContainer.appendChild(card);
        }

        const inputId = `player-name-${idx}`;
        const existingInput = card.querySelector(`#${inputId}`);
        const inputHtml = `<input type="text" id="${inputId}" ${isMe ? '' : 'disabled'} style="${isMe ? '' : 'background: rgba(0,0,0,0.15); border-color: transparent;'}">`;

        card.innerHTML = `
            <div class="player-card-num">Người chơi ${idx + 1} ${isMe ? '(Bạn)' : ''}</div>
            <button type="button" class="char-pick-btn" id="btn-pick-char-${idx}" data-index="${idx}" data-character-id="${character.id}" ${isMe ? '' : 'disabled style="cursor:not-allowed;"'}>
                <div class="selected-info">
                    <div class="selected-character-avatar" style="background-color: ${character.color}">
                        ${characterImageMarkup(character)}
                    </div>
                    <div class="selected-char-name-wrapper">
                        <span class="selected-char-name">${character.name}</span>
                        ${isMe ? '<i class="fa-solid fa-rotate" style="font-size: 0.85rem; color: #64748b; margin-left: 4px;"></i>' : ''}
                    </div>
                </div>
            </button>
            ${inputHtml}
        `;

        // If it was focused, restore the cursor / value
        if (existingInput && document.activeElement && document.activeElement.id === inputId) {
            const newInput = card.querySelector(`#${inputId}`);
            newInput.value = existingInput.value;
            newInput.focus();
            const valLength = newInput.value.length;
            newInput.setSelectionRange(valLength, valLength);
        }

        const renderedInput = card.querySelector(`#${inputId}`);
        if (renderedInput && (!existingInput || document.activeElement?.id !== inputId)) {
            renderedInput.value = p.name;
        }

        if (isMe) {
            const nameInput = card.querySelector(`#${inputId}`);
            nameInput.addEventListener("change", () => {
                const newName = nameInput.value.trim();
                if (newName) {
                    connection.invoke("UpdatePlayerName", roomCode, newName)
                        .catch(err => console.error("Error updating player name: ", err));
                }
            });
            nameInput.addEventListener("keydown", (e) => {
                if (e.key === "Enter") {
                    nameInput.blur();
                }
            });

            const btnPick = card.querySelector(".char-pick-btn");
            btnPick.addEventListener("click", () => {
                openCharacterSelectModal(idx);
            });
        }
    });
}

// Developer cheat key for testing chain reactions (Reward to Wheel Spin)
window.addEventListener("keydown", (e) => {
    if (e.key === "d" || e.key === "D") {
        if (screens.gameplay && screens.gameplay.classList.contains("active") && !isOnlineMode) {
            const player = players[activePlayerIndex];
            player.tileIndex = 4;
            updatePlayerPositionsOnBoard();
            
            const mockReward = { type: "tiến", name: "Cà Rốt Siêu Cấp Thử Nghiệm", detail: "Nhảy sang ô Vòng Quay! Tiến lên 2 ô.", value: 2 };
            triggerRewardEventForced(player, mockReward);
        }
    }
});

function triggerRewardEventForced(player, reward) {
    document.getElementById("reward-modal-desc").innerText = "Tuyệt vời! Bạn nhận được một phần quà may mắn. Hãy chọn 1 lá bài!";
    document.getElementById("reward-result-box").style.display = "none";
    document.getElementById("btn-close-reward-modal").style.display = "none";
    const cardsContainer = document.getElementById("reward-cards-container");
    cardsContainer.style.display = "flex";
    cardsContainer.innerHTML = "";

    let alternatives = REWARD_LIST.filter(r => r.name !== reward.name);
    alternatives.sort(() => Math.random() - 0.5);
    let cardChoices = [reward, alternatives[0], alternatives[1]];
    cardChoices.sort(() => Math.random() - 0.5);

    let hasSelected = false;

    cardChoices.forEach((choice, index) => {
        const card = document.createElement("div");
        card.className = "event-card";
        card.innerHTML = `
            <div class="card-back">
                <i class="fa-solid fa-gift"></i>
                <span>Thưởng ${index + 1}</span>
            </div>
            <div class="card-front">
                <div class="card-front-icon"><i class="fa-solid fa-circle-check"></i></div>
                <div class="effect-name">${escapeHTML(choice.name)}</div>
                <div class="effect-detail">${escapeHTML(choice.detail)}</div>
            </div>
        `;

        card.addEventListener("click", () => {
            if (hasSelected) return;
            hasSelected = true;

            const cardInnerElements = cardsContainer.querySelectorAll(".event-card");
            cardInnerElements.forEach((c, idx) => {
                c.classList.add("disabled");
                const frontName = c.querySelector(".effect-name");
                const frontDetail = c.querySelector(".effect-detail");
                
                if (idx === index) {
                    frontName.innerText = reward.name;
                    frontDetail.innerText = reward.detail;
                    c.classList.add("flipped");
                } else {
                    let alternativeIndex = idx < index ? idx : idx - 1;
                    frontName.innerText = alternatives[alternativeIndex].name;
                    frontDetail.innerText = alternatives[alternativeIndex].detail;
                    c.classList.add("flipped");
                    c.classList.add("faded");
                }
            });

            applyRewardEffect(player, reward);

            setTimeout(() => {
                document.getElementById("reward-modal-desc").innerText = "Chi tiết phần thưởng:";
                document.getElementById("reward-result-box").style.display = "block";
                document.getElementById("reward-result-box").innerHTML = `
                    <div class="effect-name">${escapeHTML(reward.name)}</div>
                    <div class="effect-detail">${escapeHTML(reward.detail)}</div>
                `;
                document.getElementById("btn-close-reward-modal").style.display = "inline-flex";
                document.getElementById("btn-close-reward-modal").onclick = () => {
                    document.getElementById("reward-modal").classList.remove("active");
                    const tile = TILE_COORDS[player.tileIndex];
                    if (tile.type !== "start") {
                        activateTile(player);
                    } else {
                        nextTurn();
                    }
                };
            }, 800);
        });

        cardsContainer.appendChild(card);
    });

    document.getElementById("reward-modal").classList.add("active");
}
