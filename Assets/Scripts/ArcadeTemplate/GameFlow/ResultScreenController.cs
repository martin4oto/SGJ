using UnityEngine;
using TMPro;
using ArcadesTemplate.Input;
using ArcadesTemplate.Core;
using ArcadesTemplate.Leaderboard;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace ArcadesTemplate.GameFlow
{
    /// <summary>
    /// Shared logic for the Win and Lose screens:
    /// shows the score, redirects to NameEntry for new high scores and
    /// handles free replay (A / Start = play again, B = back to start screen).
    /// </summary>
    public abstract class ResultScreenController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] protected TMP_Text scoreText;
        [SerializeField] protected TMP_Text hintText;

        [Header("Localization")]
        [SerializeField] protected LocalizedString scoreLineText;                // "SCORE: {score}"
        [SerializeField] protected LocalizedString replayFreeText;               // "PRESS A TO PLAY AGAIN"
        [SerializeField] protected LocalizedString highScoreSavedReplayFreeText; // "HIGH SCORE SAVED\nPRESS A TO PLAY AGAIN"

        [Header("Input")]
        [Tooltip("Minimum time before input is accepted (seconds). Prevents skipping the screen by accident.")]
        [SerializeField] protected float inputDelay = 0.5f;

        protected int Score { get; private set; }

        private float _timeSinceLoad;
        private bool _leaving;

        protected virtual async void Start()
        {
            var sceneMgr = ArcadeSceneManager.Instance;

            Score = sceneMgr != null ? sceneMgr.LastScore : 0;

            bool isHighScore = LeaderboardManager.Instance != null &&
                               LeaderboardManager.Instance.IsHighScore(Score);

            bool handled = sceneMgr != null && sceneMgr.HasHandledNameEntryForLastScore;

            if (isHighScore && !handled && sceneMgr != null)
            {
                _leaving = true;
                sceneMgr.LoadNameEntryScreen();
                return;
            }

            await LocalizationSettings.InitializationOperation.Task;

            if (this == null)
                return; // scene was unloaded while waiting

            if (scoreText != null)
                scoreText.text = L(scoreLineText, new { score = Score }, $"SCORE: {Score}");

            bool hasSavedHighScore = sceneMgr != null && sceneMgr.HasSavedHighScoreForLastScore;

            if (hintText != null)
            {
                hintText.text = (isHighScore && hasSavedHighScore)
                    ? L(highScoreSavedReplayFreeText, null, "HIGH SCORE SAVED\nPRESS A TO PLAY AGAIN")
                    : L(replayFreeText, null, "PRESS A TO PLAY AGAIN");
            }
        }

        protected virtual void Update()
        {
            if (_leaving)
                return;

            _timeSinceLoad += Time.unscaledDeltaTime;
            if (_timeSinceLoad < inputDelay)
                return;

            var input = ArcadeInput.Instance;
            var sceneMgr = ArcadeSceneManager.Instance;
            if (input == null || sceneMgr == null)
                return;

            if (input.ButtonStartPressedThisFrame || input.ButtonAPressedThisFrame)
            {
                _leaving = true;
                sceneMgr.Replay();
            }
            else if (input.ButtonBPressedThisFrame)
            {
                _leaving = true;
                sceneMgr.LoadStartScreen();
            }
        }

        protected static string L(LocalizedString ls, object args, string fallback)
        {
            if (ls == null || ls.IsEmpty)
                return fallback;

            ls.Arguments = args != null ? new object[] { args } : null;
            return ls.GetLocalizedString();
        }
    }
}
