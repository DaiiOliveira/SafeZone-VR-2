using System.Collections.Generic;

namespace SafeZoneVR
{
    public class ScenarioResult
    {
        public float elapsedSeconds;
        public int completedCount;
        public int totalSteps;
        public int outOfOrderCount;
        public int repeatCount;
        public int mistakeCount;
        public readonly List<string> mistakes = new List<string>();
        public readonly List<string> notes = new List<string>();
        public int stars;
        public int score;
        public float bestTimeSeconds = -1f;
        public int bestStars;
        public bool isNewBest;

        public int penaltyCount => outOfOrderCount + mistakeCount;

        public static string FormatTime(float seconds)
        {
            if (seconds < 0f) seconds = 0f;
            var m = (int)(seconds / 60f);
            var s = (int)(seconds % 60f);
            return m > 0 ? $"{m}m {s:00}s" : $"{s}s";
        }
    }
}
