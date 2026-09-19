using UnityEngine;

namespace SafeZoneVR
{
    public static class PerformanceEvaluator
    {
        public const int k_StartScore = 100;
        public const int k_LightPenalty = 5;
        public const int k_GravePenalty = 25;
        public const int k_TimePenalty = 10;

        const string k_BestTimeKey = "safezone.best.time.";
        const string k_BestScoreKey = "safezone.best.score.";
        const string k_BestStarsKey = "safezone.best.stars.";

        public static void Evaluate(ScenarioSO scenario, ScenarioResult result)
        {
            var light = 0;
            var grave = 0;
            for (var i = 0; i < result.actions.Count; i++)
            {
                var a = result.actions[i];
                if (a.kind == ActionKind.ErroLeve) light++;
                else if (a.kind == ActionKind.ErroGrave) grave++;
            }
            light += result.outOfOrderCount;

            result.lightMistakes = light;
            result.graveMistakes = grave;
            result.overTimeLimit = scenario != null && result.elapsedSeconds > scenario.timeLimitSeconds;

            var score = k_StartScore - light * k_LightPenalty - grave * k_GravePenalty;
            if (result.overTimeLimit)
                score -= k_TimePenalty;
            result.score = Mathf.Clamp(score, 0, k_StartScore);

            if (!result.success)
            {
                result.classification = "Insuficiente";
                result.recommendation = "A fase foi interrompida. Repita para praticar o procedimento seguro.";
                result.stars = 0;
            }
            else if (result.score >= 90)
            {
                result.classification = "Excelente";
                result.recommendation = "";
                result.stars = 3;
            }
            else if (result.score >= 70)
            {
                result.classification = "Adequado";
                result.recommendation = "";
                result.stars = 2;
            }
            else if (result.score >= 50)
            {
                result.classification = "Requer revisão";
                result.recommendation = "Reveja as orientações abaixo antes de repetir.";
                result.stars = 1;
            }
            else
            {
                result.classification = "Insuficiente";
                result.recommendation = "Recomendamos repetir a fase.";
                result.stars = 0;
            }

            if (scenario == null)
                return;

            var id = scenario.scenarioId;
            result.bestScore = PlayerPrefs.GetInt(k_BestScoreKey + id, -1);
            result.bestTimeSeconds = PlayerPrefs.GetFloat(k_BestTimeKey + id, -1f);

            if (!result.success)
                return;

            var betterScore = result.score > result.bestScore;
            var betterTime = result.bestTimeSeconds < 0f || result.elapsedSeconds < result.bestTimeSeconds;
            result.isNewBest = betterScore || (result.score == result.bestScore && betterTime);

            if (betterScore)
            {
                PlayerPrefs.SetInt(k_BestScoreKey + id, result.score);
                PlayerPrefs.SetInt(k_BestStarsKey + id, result.stars);
            }
            if (betterTime)
                PlayerPrefs.SetFloat(k_BestTimeKey + id, result.elapsedSeconds);
            if (betterScore || betterTime)
                PlayerPrefs.Save();
        }

        public static int GetBestScore(ScenarioSO scenario)
        {
            return scenario != null ? PlayerPrefs.GetInt(k_BestScoreKey + scenario.scenarioId, -1) : -1;
        }

        public static int GetBestStars(ScenarioSO scenario)
        {
            return scenario != null ? PlayerPrefs.GetInt(k_BestStarsKey + scenario.scenarioId, 0) : 0;
        }

        public static float GetBestTime(ScenarioSO scenario)
        {
            return scenario != null ? PlayerPrefs.GetFloat(k_BestTimeKey + scenario.scenarioId, -1f) : -1f;
        }

        public static string Classify(int score)
        {
            if (score >= 90) return "Excelente";
            if (score >= 70) return "Adequado";
            if (score >= 50) return "Requer revisão";
            return "Insuficiente";
        }
    }
}
