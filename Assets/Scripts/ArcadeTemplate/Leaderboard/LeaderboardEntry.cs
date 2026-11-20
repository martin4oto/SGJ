using System;

namespace ArcadesTemplate.Leaderboard
{
    [Serializable]
    public class LeaderboardEntry
    {
        public string name;
        public int score;
        public string dateIso;   // stored as ISO string (UTC)

        public DateTime DateUtc =>
            DateTime.TryParse(dateIso, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var dt)
                ? dt
                : DateTime.MinValue;
    }
}