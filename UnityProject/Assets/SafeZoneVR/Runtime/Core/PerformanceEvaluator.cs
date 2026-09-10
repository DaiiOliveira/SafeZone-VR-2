using UnityEngine;

namespace SafeZoneVR
{
    /// <summary>
    /// Converte tempo e erros em estrelas/pontuação e guarda o melhor resultado por cenário (PlayerPrefs).
    /// Regra simples e transparente para o jogador:
    ///  3 estrelas = concluiu dentro do tempo-alvo sem erros;
    ///  2 estrelas = concluiu dentro do tempo "bom" com até 2 erros;
    ///  1 estrela  = concluiu.
    /// </summary>
    public static class PerformanceEvaluator
    {
        const string k_BestTimeKey = "safezone.best.time.";
        const string k_BestStarsKey = "safezone.best.stars.";

        public static void Evaluate(ScenarioSO scenario, ScenarioResult result)
        {
            var completed = result.totalSteps > 0 && result.completedCount >= result.totalSteps;
            var penalties = result.penaltyCount;

            if (!completed)
                result.stars = 0;
            else if (result.elapsedSeconds <= scenario.targetTimeSeconds && penalties == 0)
                result.stars = 3;
            else if (result.elapsedSeconds <= scenario.goodTimeSeconds && penalties <= 2)
                result.stars = 2;
            else
                result.stars = 1;

            var timeScore = Mathf.Clamp(1000f - result.elapsedSeconds * 1.5f, 200f, 1000f);
            result.score = Mathf.Max(0, Mathf.RoundToInt(timeScore - penalties * 100f - result.repeatCount * 10f));

            var id = scenario.scenarioId;
            var prevTime = PlayerPrefs.GetFloat(k_BestTimeKey + id, -1f);
            var prevStars = PlayerPrefs.GetInt(k_BestStarsKey + id, 0);
            result.bestTimeSeconds = prevTime;
            result.bestStars = prevStars;

            if (completed)
            {
                var betterStars = result.stars > prevStars;
                var betterTime = prevTime < 0f || result.elapsedSeconds < prevTime;
                result.isNewBest = betterStars || (result.stars == prevStars && betterTime);

                if (betterStars || betterTime)
                {
                    if (betterTime) PlayerPrefs.SetFloat(k_BestTimeKey + id, result.elapsedSeconds);
                    if (betterStars) PlayerPrefs.SetInt(k_BestStarsKey + id, result.stars);
                    PlayerPrefs.Save();
                }
            }
        }

        public static int GetBestStars(ScenarioSO scenario)
        {
            return scenario != null ? PlayerPrefs.GetInt(k_BestStarsKey + scenario.scenarioId, 0) : 0;
        }

        public static float GetBestTime(ScenarioSO scenario)
        {
            return scenario != null ? PlayerPrefs.GetFloat(k_BestTimeKey + scenario.scenarioId, -1f) : -1f;
        }
    }
}
