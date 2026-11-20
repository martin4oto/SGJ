using System;
using System.Collections.Generic;

namespace ArcadesTemplate.Leaderboard
{
    [Serializable]
    public class LeaderboardData
    {
        public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
    }
}