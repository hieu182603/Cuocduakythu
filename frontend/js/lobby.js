// Lobby rendering
let activePickCardIndex = -1;

function characterImageMarkup(character, className = "selected-char-image") {
    const wideClass = character.imageClass ? ` ${character.imageClass}` : "";
    return `<img class="${className}${wideClass}" src="${character.image}" alt="${character.name}" loading="lazy">`;
}

function initLobby() {
    let count = parseInt(document.getElementById("player-count-display").innerText);
    renderPlayerSetupCards(count);
}

function renderPlayerSetupCards(count) {
    const listContainer = document.getElementById("players-setup-list");
    listContainer.innerHTML = "";

    // Load last names from local storage if available
    let storedNames = [];
    try {
        storedNames = JSON.parse(localStorage.getItem("last_player_names") || "[]");
    } catch(e) {}

    for (let i = 0; i < count; i++) {
        let defaultName = storedNames[i] || `Người chơi ${i + 1}`;
        // Phân bổ mặc định theo vòng; người chơi vẫn có thể chọn trùng nhân vật.
        let defaultChar = CHARACTER_DATABASE[i % CHARACTER_DATABASE.length];

        const card = document.createElement("div");
        card.className = "player-setup-card";
        card.id = `player-setup-card-${i}`;
        card.innerHTML = `
            <div class="player-card-num">Người chơi ${i + 1}</div>
            <button type="button" class="char-pick-btn" id="btn-pick-char-${i}" data-index="${i}" data-character-id="${defaultChar.id}">
                <div class="selected-info">
                    <div class="selected-character-avatar" style="background-color: ${defaultChar.color}">
                        ${characterImageMarkup(defaultChar)}
                    </div>
                    <div class="selected-char-name-wrapper">
                        <span class="selected-char-name">${defaultChar.name}</span>
                        <i class="fa-solid fa-rotate" style="font-size: 0.85rem; color: #64748b; margin-left: 4px;"></i>
                    </div>
                </div>
            </button>
            <input type="text" id="player-name-${i}" value="${defaultName}" placeholder="Nhập tên người chơi">
        `;

        listContainer.appendChild(card);
        
        // Attach horse select action
        const btnPick = card.querySelector(".char-pick-btn");
        btnPick.addEventListener("click", (e) => {
            let pIdx = parseInt(e.currentTarget.dataset.index);
            openCharacterSelectModal(pIdx);
        });
    }
}

function openCharacterSelectModal(playerIndex) {
    activePickCardIndex = playerIndex;
    const grid = document.getElementById("char-grid");
    grid.innerHTML = "";

    CHARACTER_DATABASE.forEach(char => {
        let isSelected = false;
        
        let activeCardBtn = document.getElementById(`btn-pick-char-${playerIndex}`);
        if(activeCardBtn) {
            let activeName = activeCardBtn.querySelector(".selected-char-name").innerText;
            isSelected = activeName === char.name;
        }

        const charCard = document.createElement("button");
        charCard.type = "button";
        charCard.className = `char-card ${isSelected ? 'selected' : ''}`;
        charCard.setAttribute("aria-pressed", String(isSelected));
        charCard.innerHTML = `
            <div class="char-card-avatar" style="background-color: ${char.color}">
                ${characterImageMarkup(char, "char-card-image")}
            </div>
            <div class="char-card-name">${char.name}</div>
            <div class="char-card-badge">${char.badge}</div>
        `;

        charCard.addEventListener("click", () => {
            selectHorse(playerIndex, char);
            document.getElementById("char-select-modal").classList.remove("active");
        });

        grid.appendChild(charCard);
    });

    document.getElementById("char-select-modal").classList.add("active");
}

function selectHorse(playerIndex, character) {
    const cardBtn = document.getElementById(`btn-pick-char-${playerIndex}`);
    if (cardBtn) {
        cardBtn.dataset.characterId = character.id;
        const avatar = cardBtn.querySelector(".selected-character-avatar");
        const image = cardBtn.querySelector(".selected-char-image");
        avatar.style.backgroundColor = character.color;
        image.src = character.image;
        image.alt = character.name;
        image.className = `selected-char-image${character.imageClass ? ` ${character.imageClass}` : ""}`;
        cardBtn.querySelector(".selected-char-name").innerText = character.name;
    }

    if (typeof isOnlineMode !== "undefined" && isOnlineMode && connection && connection.state === "Connected") {
        connection.invoke("UpdatePlayerHorse", roomCode, character.id)
            .catch(err => {
                console.error("Error updating horse: ", err);
                showNotification("Không thể cập nhật nhân vật. Vui lòng thử lại.", "error");
            });
    }
}

// Question Pool screen
function openQuestionsPool() {
    const list = document.getElementById("qpool-list");
    list.innerHTML = "";
    
    questions.forEach((q, idx) => {
        const item = document.createElement("div");
        item.className = "qpool-item";
        
        let answersHtml = q.answers.map((ans, aIdx) => `
            <div class="qpool-answer ${aIdx === q.correct ? 'correct-answer' : ''}">
                ${String.fromCharCode(65 + aIdx)}. ${ans}
            </div>
        `).join("");

        item.innerHTML = `
            <div class="qpool-question">${idx + 1}. ${q.question}</div>
            <div class="qpool-answers">
                ${answersHtml}
            </div>
        `;
        list.appendChild(item);
    });

    document.getElementById("questions-pool-modal").classList.add("active");
}

// Reset states
function resetGameStates() {
    players = [];
    activePlayerIndex = 0;
    isGameEnding = false;
}

// GAMEPLAY LOGIC
function startGame() {
    if (questions.length === 0) {
        showNotification("Chưa tải được câu hỏi từ database. Không thể bắt đầu cuộc đua.", "error");
        return;
    }

    resetGameStates();
    const count = parseInt(document.getElementById("player-count-display").innerText);
    const storedNames = [];

    // Parse players from setup cards
    for (let i = 0; i < count; i++) {
        let nameInput = document.getElementById(`player-name-${i}`);
        let name = nameInput.value.trim() || `Người chơi ${i + 1}`;
        storedNames.push(name);

        let charBtn = document.getElementById(`btn-pick-char-${i}`);
        let charName = charBtn.querySelector(".selected-char-name").innerText;
        let character = CHARACTER_DATABASE.find(c => c.name === charName);

        players.push({
            id: i,
            name: name,
            character: character,
            tileIndex: 0,        // Start at tile 0 (START/FINISH)
            wrongStreak: 0,      // Consecutive wrong answers
            shield: false,       // Has active shield
            skipTurn: false,     // Skip next turn
            doubleDice: false,   // Next roll has 2 dice
            diceModifier: 0,     // Decrease dice range timer (number of turns)
            lapCompleted: false
        });
    }

    // Save names to local storage for convenience
    localStorage.setItem("last_player_names", JSON.stringify(storedNames));

    // Render Board
    renderBoard();

    // Render initial players onto Tile 1 (START)
    updatePlayerPositionsOnBoard();

    // Render Scoreboard
    renderScoreboard();

    // Initialize Turn
    activePlayerIndex = 0;
    setupTurn(activePlayerIndex);

    showScreen("gameplay");
    logMessage("Cuộc đua đã bắt đầu! Đang ở vạch xuất phát.", "log-win");
}
