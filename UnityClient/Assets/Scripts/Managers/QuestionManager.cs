using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using CuocDuaKyThu.Data;
using CuocDuaKyThu.Utilities;

namespace CuocDuaKyThu.Managers
{
    /// <summary>
    /// Responsible for loading questions from Supabase via the backend API.
    /// Caches questions locally. Serves random questions during gameplay.
    /// Handles answer submission and wrong-streak penalty application.
    /// 
    /// RULE: Questions MUST come from Supabase. NO JSON files, NO hardcoding.
    /// </summary>
    public class QuestionManager : MonoBehaviour
    {
        private List<QuestionDTO> _cachedQuestions = new();
        private QuestionDTO _currentQuestion;
        private System.Random _random = new();

        public int TotalQuestions => _cachedQuestions.Count;
        public QuestionDTO CurrentQuestion => _currentQuestion;

        // ════════════════════════════════════════
        // LOADING FROM API
        // ════════════════════════════════════════

        /// <summary>
        /// Load all questions from the backend REST API (which reads from Supabase).
        /// Must be called before gameplay starts.
        /// </summary>
        public void LoadQuestionsFromApi(Action onComplete)
        {
            string url = Constants.DefaultServerUrl + Constants.ApiQuestionsEndpoint;
            StartCoroutine(FetchQuestions(url, onComplete));
        }

        private IEnumerator FetchQuestions(string url, Action onComplete)
        {
            using var request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Accept", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;

                // Unity's JsonUtility doesn't handle top-level arrays, wrap it
                string wrappedJson = "{\"questions\":" + json + "}";
                var wrapper = JsonUtility.FromJson<QuestionListWrapper>(wrappedJson);

                if (wrapper?.questions != null)
                {
                    _cachedQuestions = wrapper.questions;
                    Debug.Log($"[QuestionManager] Loaded {_cachedQuestions.Count} questions from Supabase.");
                }
                else
                {
                    Debug.LogWarning("[QuestionManager] Failed to parse questions from API response.");
                }
            }
            else
            {
                Debug.LogError($"[QuestionManager] Failed to load questions: {request.error}");
                // No fallback — questions MUST come from Supabase per spec
            }

            onComplete?.Invoke();
        }

        // ════════════════════════════════════════
        // QUESTION SERVING
        // ════════════════════════════════════════

        /// <summary>Get a random question from the cached pool.</summary>
        public QuestionDTO GetRandomQuestion()
        {
            if (_cachedQuestions.Count == 0) return null;
            int idx = _random.Next(_cachedQuestions.Count);
            _currentQuestion = _cachedQuestions[idx];
            return _currentQuestion;
        }

        // ════════════════════════════════════════
        // QUESTION EVENT (OFFLINE)
        // ════════════════════════════════════════

        /// <summary>Trigger a question popup for the given player (offline mode).</summary>
        public void TriggerQuestionEvent(PlayerState player)
        {
            var question = GetRandomQuestion();
            if (question == null)
            {
                GameManager.Instance.uiManager.LogMessage(
                    "Không có câu hỏi nào trong kho. Bỏ qua lượt hỏi.", UIManager.LogType.Question);
                Invoke(nameof(AdvanceTurn), Constants.AnswerShowDelay);
                return;
            }

            // Show question popup via UIManager
            GameManager.Instance.uiManager.ShowQuestionPopup(question, player.wrongStreak, (selectedIndex) =>
            {
                HandleAnswer(player, question, selectedIndex);
            });
        }

        // ════════════════════════════════════════
        // ANSWER PROCESSING
        // ════════════════════════════════════════

        private void HandleAnswer(PlayerState player, QuestionDTO question, int selectedIndex)
        {
            bool isCorrect = (selectedIndex == question.CorrectIndex);

            if (isCorrect)
            {
                player.wrongStreak = 0;
                GameManager.Instance.uiManager.LogMessage(
                    $"[{player.name}] đã trả lời ĐÚNG câu hỏi trắc nghiệm!", UIManager.LogType.Question);

                // Close popup after delay, then next turn
                StartCoroutine(DelayedAction(Constants.AnswerShowDelay, () =>
                {
                    GameManager.Instance.uiManager.HideQuestionPopup();
                    GameManager.Instance.turnManager.NextTurn();
                }));
            }
            else
            {
                player.wrongStreak++;
                string penaltyDesc = ApplyWrongAnswerPenalty(player);

                GameManager.Instance.uiManager.LogMessage(
                    $"[{player.name}] trả lời SAI! Chuỗi sai liên tiếp: {player.wrongStreak}. Hình phạt: {penaltyDesc}.",
                    UIManager.LogType.Trap);

                StartCoroutine(DelayedAction(Constants.WrongAnswerShowDelay, () =>
                {
                    GameManager.Instance.uiManager.HideQuestionPopup();
                    GameManager.Instance.boardManager.TeleportToken(player);
                    GameManager.Instance.turnManager.NextTurn();
                }));
            }
        }

        /// <summary>Apply a random penalty for wrong answer. Returns description text.</summary>
        private string ApplyWrongAnswerPenalty(PlayerState player)
        {
            int penaltyType = _random.Next(3);

            switch (penaltyType)
            {
                case 0: // Lùi ô
                    int backSteps = _random.Next(1, 4); // 1–3
                    player.tileIndex = Mathf.Max(0, player.tileIndex - backSteps);
                    return $"lùi {backSteps} ô";

                case 1: // Mất lượt
                    player.skipTurn = true;
                    return "mất lượt ở vòng tiếp theo";

                case 2: // Chờ
                    int seconds = Constants.BaseWaitPenaltySeconds + (player.wrongStreak * Constants.WrongStreakBonusSeconds);
                    return $"chờ thêm {seconds} giây hình phạt";

                default:
                    return "";
            }
        }

        private void AdvanceTurn()
        {
            GameManager.Instance.turnManager.NextTurn();
        }

        private IEnumerator DelayedAction(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
    }
}
