using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcadesTemplate.Leaderboard
{
    [DefaultExecutionOrder(-50)]
    public class LeaderboardManager : MonoBehaviour
    {
        public static LeaderboardManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private int maxEntries = 10;
        [SerializeField] private string fileName = "leaderboard.json";

        private LeaderboardData _data = new LeaderboardData();
        private string _filePath;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _filePath = Path.Combine(Application.persistentDataPath, fileName);
            LoadFromDisk();
        }

        // ------------- Public API -------------

        /// <summary>
        /// Adds a score and saves if it belongs in the top table.
        /// </summary>
        public void AddScore(string name, int score)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = "AAA"; // default initials

            var entry = new LeaderboardEntry
            {
                name = name.Trim(),
                score = score,
                dateIso = DateTime.UtcNow.ToString("o")
            };

            _data.entries.Add(entry);

            // Sort descending by score, then by date (newer first)
            _data.entries = _data.entries
                .OrderByDescending(e => e.score)
                .ThenByDescending(e => e.dateIso)
                .ToList();

            // Keep only top N
            if (_data.entries.Count > maxEntries)
                _data.entries.RemoveRange(maxEntries, _data.entries.Count - maxEntries);

            SaveToDisk();
        }

        /// <summary>
        /// Returns true if this score would be in the top table.
        /// </summary>
        public bool IsHighScore(int score)
        {
            if (_data.entries.Count < maxEntries)
                return true;

            int lowestTopScore = _data.entries.Last().score;
            return score > lowestTopScore;
        }

        /// <summary>
        /// Returns a read-only view of the current leaderboard (already sorted).
        /// </summary>
        public IReadOnlyList<LeaderboardEntry> GetTopScores()
        {
            return _data.entries;
        }
        
        // Get top score
        public int GetTopScore()
        {
            if (_data.entries.Count == 0)
                return 0;

            return _data.entries[0].score;
        }

        /// <summary>
        /// Clears all scores (useful for debug / settings menu).
        /// </summary>
        public void ClearAllScores()
        {
            _data.entries.Clear();
            SaveToDisk();
        }

        // ------------- Persistence -------------

        private void LoadFromDisk()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    _data = new LeaderboardData();
                    return;
                }

                string json = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    _data = new LeaderboardData();
                    return;
                }

                _data = JsonUtility.FromJson<LeaderboardData>(json) ?? new LeaderboardData();

                // Make sure it’s sorted & trimmed
                _data.entries = _data.entries
                    .OrderByDescending(e => e.score)
                    .ThenByDescending(e => e.dateIso)
                    .Take(maxEntries)
                    .ToList();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LeaderboardManager] Failed to load: {ex.Message}");
                _data = new LeaderboardData();
            }
        }

        private void SaveToDisk()
        {
            try
            {
                string json = JsonUtility.ToJson(_data, true);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LeaderboardManager] Failed to save: {ex.Message}");
            }
        }
    }
}
