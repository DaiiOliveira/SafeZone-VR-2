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
        public bool success = true;
        public string failureReason;
        public readonly List<ActionRecord> actions = new List<ActionRecord>();
        public readonly List<string> notes = new List<string>();

        public int score;
        public int lightMistakes;
        public int graveMistakes;
        public bool overTimeLimit;
        public string classification = "";
        public string recommendation = "";
        public int stars;
        public float bestTimeSeconds = -1f;
        public int bestScore = -1;
        public bool isNewBest;

        public int mistakeCount
        {
            get
            {
                var n = 0;
                for (var i = 0; i < actions.Count; i++)
                    if (actions[i].isMistake) n++;
                return n;
            }
        }

        public static string FormatTime(float seconds)
        {
            if (seconds < 0f) seconds = 0f;
            var m = (int)(seconds / 60f);
            var s = (int)(seconds % 60f);
            return m > 0 ? $"{m}m {s:00}s" : $"{s}s";
        }
    }
}
