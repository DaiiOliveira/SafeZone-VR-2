using System.Collections.Generic;
using UnityEngine;

namespace SafeZoneVR
{
    [CreateAssetMenu(fileName = "Scenario", menuName = "SafeZone VR/Scenario", order = 0)]
    public class ScenarioSO : ScriptableObject
    {
        [SerializeField]
        string m_ScenarioId = "scenario_001";

        [SerializeField]
        string m_DisplayName = "Scenario Name";

        [SerializeField]
        [TextArea(2, 5)]
        string m_Description = "";

        [SerializeField]
        [Tooltip("Nome exato da cena (deve estar em Build Settings).")]
        string m_SceneName = "";

        [SerializeField]
        [Tooltip("Cenário jogável? Se falso, aparece no menu como 'Em breve'.")]
        bool m_IsAvailable = true;

        [SerializeField]
        [Tooltip("Tempo (s) para 3 estrelas sem erros.")]
        float m_TargetTimeSeconds = 240f;

        [SerializeField]
        [Tooltip("Tempo (s) para 2 estrelas.")]
        float m_GoodTimeSeconds = 420f;

        [SerializeField]
        [Tooltip("Tempo-limite (s) do cenário. Passar dele desconta pontos (RN04), mas não encerra a fase.")]
        float m_TimeLimitSeconds = 420f;

        [SerializeField]
        List<MissionStepSO> m_Steps = new List<MissionStepSO>();

        [SerializeField]
        List<EducationCardSO> m_RecapCards = new List<EducationCardSO>();

        public string scenarioId => m_ScenarioId;
        public string displayName => m_DisplayName;
        public string description => m_Description;
        public string sceneName => m_SceneName;
        public bool isAvailable => m_IsAvailable;
        public float targetTimeSeconds => m_TargetTimeSeconds;
        public float goodTimeSeconds => m_GoodTimeSeconds;
        public float timeLimitSeconds => m_TimeLimitSeconds > 0f ? m_TimeLimitSeconds : m_GoodTimeSeconds;
        public IReadOnlyList<MissionStepSO> steps => m_Steps;
        public IReadOnlyList<EducationCardSO> recapCards => m_RecapCards;

#if UNITY_EDITOR
        public void EditorInitialize(string scenarioId, string displayName, List<MissionStepSO> steps, List<EducationCardSO> recapCards)
        {
            m_ScenarioId = scenarioId;
            m_DisplayName = displayName;
            m_Steps = steps;
            m_RecapCards = recapCards;
        }

        public void EditorInitialize(string scenarioId, string displayName, string description, string sceneName,
            bool isAvailable, float targetTime, float goodTime, List<MissionStepSO> steps, List<EducationCardSO> recapCards)
        {
            m_ScenarioId = scenarioId;
            m_DisplayName = displayName;
            m_Description = description;
            m_SceneName = sceneName;
            m_IsAvailable = isAvailable;
            m_TargetTimeSeconds = targetTime;
            m_GoodTimeSeconds = goodTime;
            m_Steps = steps;
            m_RecapCards = recapCards;
        }

        public void EditorSetTimeLimit(float seconds) => m_TimeLimitSeconds = seconds;
#endif
    }
}
