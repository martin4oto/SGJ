using UnityEngine;
using TMPro;

using UnityEngine.Localization;
using UnityEngine.Localization.Settings;


namespace ArcadesTemplate.Leaderboard
{
    public class LeaderboardTableUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Transform rowsParent;   // RowsContainer
        [SerializeField] private GameObject rowTemplate; // RowTemplate (disabled)
        [SerializeField] private int maxDisplayedRows = 10;
        [SerializeField] private TMP_Text emptyText;     // optional "NO SCORES YET"
        
        public LocalizedString noScoresText;

        private void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (rowsParent == null || rowTemplate == null)
            {
                Debug.LogError("[LeaderboardTableUI] rowsParent or rowTemplate not set.");
                return;
            }

            // Remove old rows (leave template)
            foreach (Transform child in rowsParent)
            {
                if (child == rowTemplate.transform) continue;
                Destroy(child.gameObject);
            }

            if (LeaderboardManager.Instance == null)
            {
                if (emptyText != null)
                {
                    emptyText.gameObject.SetActive(true);
                    emptyText.text = "NO LEADERBOARD MANAGER";
                }
                return;
            }

            var scores = LeaderboardManager.Instance.GetTopScores();
            if (scores == null || scores.Count == 0)
            {
                if (emptyText != null)
                {
                    emptyText.gameObject.SetActive(true);
                    emptyText.text = L(noScoresText);
                }
                return;
            }

            if (emptyText != null)
                emptyText.gameObject.SetActive(false);

            int rank = 1;
            foreach (var entry in scores)
            {
                if (rank > maxDisplayedRows)
                    break;

                var rowObj = Instantiate(rowTemplate, rowsParent);
                rowObj.SetActive(true);

                // If using 3 child texts: Rank / Name / Score
                var texts = rowObj.GetComponentsInChildren<TMP_Text>();

                if (texts.Length >= 3)
                {
                    texts[0].text = $"#{rank}";
                    texts[1].text = entry.name;
                    texts[2].text = entry.score.ToString("D6");
                }
                else if (texts.Length == 1)
                {
                    // Single text style: "#1 AAA 12345"
                    texts[0].text = $"#{rank}  {entry.name}  {entry.score:D6}";
                }

                rank++;
            }
        }
        
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
