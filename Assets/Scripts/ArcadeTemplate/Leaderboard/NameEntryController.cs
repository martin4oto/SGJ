using UnityEngine;
using TMPro;
using ArcadesTemplate.Input;
using ArcadesTemplate.Core;
using System.Collections.Generic;

using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace ArcadesTemplate.Leaderboard
{
    public class NameEntryController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private TMP_Text[] letterTexts; // size 3

        [Header("Localization")]
        public LocalizedString scoreLineText;      // e.g. "SCORE: {score}"
        public LocalizedString hintLineText;       // e.g. "MOVE: CHANGE LETTER / POSITION\nA: OK   B: BACK"
        public LocalizedString offensiveNameText;  // e.g. "OFFENSIVE NAME - CHANGE LETTERS"

        [Header("Visuals")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = Color.green;
        [SerializeField] private float normalScale = 1f;
        [SerializeField] private float selectedScale = 1.2f;

        [Header("Input Timing")]
        [SerializeField] private float moveRepeatDelay = 0.18f;
        [SerializeField] private float deadZone = 0.4f;

        private readonly char[] _alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
        private int[] _indices = new int[3];
        private int _selectedPos = 0;
        private float _lastMoveTime = 0f;
        private int _score = 0;

        // ---------------- PROFANITY FILTER DATA ----------------
        private static bool _badWordsInitialized = false;
        private static HashSet<string> _badWords;

        private static readonly Dictionary<char, char[]> _leetMap = new Dictionary<char, char[]>
        {
            { 'A', new[] { '4' } },
            { 'B', new[] { '8' } },
            { 'E', new[] { '3' } },
            { 'I', new[] { '1' } },
            { 'O', new[] { '0' } },
            { 'S', new[] { '5' } },
            { 'G', new[] { '6' } },
            { 'T', new[] { '7' } }
        };
        // -------------------------------------------------------

        private async void Start()
        {
            InitBadWordsIfNeeded();

            if (ArcadeSceneManager.Instance != null)
                _score = ArcadeSceneManager.Instance.LastScore;

            // Ensure Localization is ready before we set any localized text
            await LocalizationSettings.InitializationOperation.Task;

            if (scoreText != null)
                scoreText.text = L(scoreLineText, new { score = _score });

            if (hintText != null)
                hintText.text = L(hintLineText);

            for (int i = 0; i < _indices.Length; i++)
                _indices[i] = 0;

            UpdateLettersVisual();
        }

        private void Update()
        {
            if (ArcadeInput.Instance == null)
                return;

            HandleStick();
            HandleButtons();
        }

        private void HandleStick()
        {
            Vector2 move = ArcadeInput.Instance.Move;
            float now = Time.unscaledTime;

            if (now - _lastMoveTime < moveRepeatDelay)
                return;

            bool moved = false;

            if (move.y > deadZone)
            {
                IncrementLetter(1);
                moved = true;
            }
            else if (move.y < -deadZone)
            {
                IncrementLetter(-1);
                moved = true;
            }

            if (move.x < -deadZone)
            {
                ChangePosition(-1);
                moved = true;
            }
            else if (move.x > deadZone)
            {
                ChangePosition(1);
                moved = true;
            }

            if (moved)
            {
                _lastMoveTime = now;
                UpdateLettersVisual();
            }
        }

        private void HandleButtons()
        {
            if (ArcadeInput.Instance.ButtonAPressedThisFrame)
            {
                if (_selectedPos < _indices.Length - 1)
                {
                    _selectedPos++;
                    UpdateLettersVisual();
                }
                else
                {
                    ConfirmName();
                }
            }

            if (ArcadeInput.Instance.ButtonBPressedThisFrame)
            {
                // Back to the last result screen (win/lose), no credit cost
                var sceneMgr = ArcadeSceneManager.Instance;
                if (sceneMgr != null)
                {
                    sceneMgr.HasHandledNameEntryForLastScore = true;
                    sceneMgr.ReturnToLastResultScreen();
                }
            }
        }

        private void IncrementLetter(int direction)
        {
            int idx = _indices[_selectedPos];
            idx += direction;

            if (idx < 0) idx = _alphabet.Length - 1;
            if (idx >= _alphabet.Length) idx = 0;

            _indices[_selectedPos] = idx;
        }

        private void ChangePosition(int direction)
        {
            _selectedPos += direction;
            if (_selectedPos < 0) _selectedPos = _indices.Length - 1;
            if (_selectedPos >= _indices.Length) _selectedPos = 0;
        }

        private void UpdateLettersVisual()
        {
            for (int i = 0; i < letterTexts.Length; i++)
            {
                if (letterTexts[i] == null)
                    continue;

                char c = _alphabet[_indices[i]];
                letterTexts[i].text = c.ToString();

                bool selected = (i == _selectedPos);
                letterTexts[i].color = selected ? selectedColor : normalColor;
                letterTexts[i].transform.localScale =
                    Vector3.one * (selected ? selectedScale : normalScale);
            }
        }

        private void ConfirmName()
        {
            string name = new string(new[]
            {
                _alphabet[_indices[0]],
                _alphabet[_indices[1]],
                _alphabet[_indices[2]]
            });

            name = name.ToUpperInvariant();

            // ---------------- PROFANITY CHECK ----------------
            if (IsProfane(name))
            {
                if (hintText != null)
                    hintText.text = L(offensiveNameText);
                // Do NOT save or leave screen
                return;
            }
            // --------------------------------------------------

            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.AddScore(name, _score);
            }

            var sceneMgr = ArcadeSceneManager.Instance;
            if (sceneMgr != null)
            {
                sceneMgr.HasSavedHighScoreForLastScore = true;
                sceneMgr.HasHandledNameEntryForLastScore = true;
                sceneMgr.ReturnToLastResultScreen();
            }
        }

        // ---------------- PROFANITY / LEETSPEAK LOGIC ----------------

        private void InitBadWordsIfNeeded()
        {
            if (_badWordsInitialized)
                return;

            _badWordsInitialized = true;
            _badWords = new HashSet<string>();

            // Load from Resources/bad_words.txt (no extension in Load call)
            TextAsset badWordsAsset = Resources.Load<TextAsset>("bad_words");
            if (badWordsAsset == null)
            {
                Debug.LogWarning("NameEntryController: Resources/bad_words.txt not found.");
                return;
            }

            string[] lines = badWordsAsset.text.Split('\n');
            foreach (string raw in lines)
            {
                string word = raw.Trim().ToUpperInvariant();
                if (string.IsNullOrEmpty(word))
                    continue;

                // We only care about 3-character words for this name entry
                if (word.Length != 3)
                    continue;

                _badWords.Add(word);

                // Generate leetspeak variants
                foreach (var variant in GenerateLeetVariants(word))
                {
                    _badWords.Add(variant);
                }
            }
        }

        private bool IsProfane(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            InitBadWordsIfNeeded();
            name = name.ToUpperInvariant();

            // Direct match (already includes leet variants)
            if (_badWords.Contains(name))
                return true;

            // Optional: regenerate variants for the entered name too
            foreach (var variant in GenerateLeetVariants(name))
            {
                if (_badWords.Contains(variant))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Generates leetspeak variants for a 3-char word using only
        /// characters that are allowed in the alphabet (A-Z, 0-9).
        /// E.g. "ASS" -> "A5S", "AS5", "A55".
        /// </summary>
        private IEnumerable<string> GenerateLeetVariants(string word)
        {
            List<string> variants = new List<string>();

            if (string.IsNullOrEmpty(word) || word.Length != 3)
                return variants;

            char[][] options = new char[3][];

            for (int i = 0; i < 3; i++)
            {
                char c = word[i];
                c = char.ToUpperInvariant(c);

                if (_leetMap.TryGetValue(c, out var mapped))
                {
                    // Include the original character plus digit variants
                    List<char> chars = new List<char> { c };
                    chars.AddRange(mapped);
                    options[i] = chars.ToArray();
                }
                else
                {
                    options[i] = new[] { c };
                }
            }

            foreach (char c0 in options[0])
            {
                foreach (char c1 in options[1])
                {
                    foreach (char c2 in options[2])
                    {
                        string v = new string(new[] { c0, c1, c2 });
                        if (!string.Equals(v, word, System.StringComparison.Ordinal))
                        {
                            variants.Add(v);
                        }
                    }
                }
            }

            return variants;
        }

        // ---------------- LOCALIZATION HELPERS ----------------

        private string L(LocalizedString ls)
        {
            ls.Arguments = null;
            return ls.GetLocalizedString();
        }

        private string L(LocalizedString ls, object args)
        {
            ls.Arguments = new object[] { args };
            return ls.GetLocalizedString();
        }
    }
}
