// State variables
let questions = [];
let players = [];
let activePlayerIndex = 0;
let speedSetting = "normal"; // normal, fast, instant
let musicVol = 50;
let sfxVol = 70;
let vfxEnabled = true;

// Online Multiplayer state
let isOnlineMode = false;
let isHost = false;
let roomCode = "";
let connection = null;
let myPlayerId = -1; // -1: spectator, otherwise player index
let gameTimerInterval = null;
let isMovementAnimating = false;
let onlineMovementQueue = [];
let isOnlineMovementQueueRunning = false;
let onlineMovementQueueVersion = 0;

let sessionToken = localStorage.getItem("session_token");
if (!sessionToken) {
    sessionToken = (typeof crypto !== 'undefined' && crypto.randomUUID) ? crypto.randomUUID() : Math.random().toString(36).substring(2) + Date.now().toString(36);
    localStorage.setItem("session_token", sessionToken);
}

// DOM Elements
const screens = {
    splash: null,
    menu: null,
    lobby: null,
    gameplay: null,
    victory: null
};
