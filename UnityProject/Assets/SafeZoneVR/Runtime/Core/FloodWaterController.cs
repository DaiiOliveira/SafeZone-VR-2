using UnityEngine;

namespace SafeZoneVR
{
    public class FloodWaterController : MonoBehaviour
    {
        [SerializeField]
        ScenarioManager m_ScenarioManager;

        [SerializeField]
        Transform m_WaterSurface;

        [SerializeField]
        float m_StartLevel = -0.04f;

        [SerializeField]
        float m_MaxLevel = 0.32f;

        [SerializeField]
        [Tooltip("Segundos para ir do nível inicial ao máximo.")]
        float m_RiseDuration = 360f;

        [SerializeField]
        [Tooltip("Ondulação sutil (metros).")]
        float m_Ripple = 0.006f;

        float m_Elapsed;
        bool m_Rising;
        bool m_Frozen;

        public ScenarioManager scenarioManager { get => m_ScenarioManager; set => m_ScenarioManager = value; }
        public Transform waterSurface { get => m_WaterSurface; set => m_WaterSurface = value; }
        public float startLevel { get => m_StartLevel; set => m_StartLevel = value; }
        public float maxLevel { get => m_MaxLevel; set => m_MaxLevel = value; }
        public float riseDuration { get => m_RiseDuration; set => m_RiseDuration = value; }

        public float currentLevel { get; private set; }

        public float normalizedLevel => Mathf.InverseLerp(m_StartLevel, m_MaxLevel, currentLevel);

        void Start()
        {
            if (m_ScenarioManager == null)
                m_ScenarioManager = ScenarioManager.instance;

            currentLevel = m_StartLevel;
            ApplyLevel(0f);

            if (m_ScenarioManager != null)
            {
                m_ScenarioManager.onScenarioStarted.AddListener(OnStarted);
                m_ScenarioManager.onScenarioComplete.AddListener(OnComplete);
                if (m_ScenarioManager.hasStarted)
                    OnStarted();
            }
        }

        void OnDestroy()
        {
            if (m_ScenarioManager != null)
            {
                m_ScenarioManager.onScenarioStarted.RemoveListener(OnStarted);
                m_ScenarioManager.onScenarioComplete.RemoveListener(OnComplete);
            }
        }

        void Update()
        {
            if (m_WaterSurface == null)
                return;

            if (m_Rising && !m_Frozen)
            {
                m_Elapsed += Time.deltaTime;
                var k = m_RiseDuration <= 0f ? 1f : Mathf.Clamp01(m_Elapsed / m_RiseDuration);

                k = 1f - (1f - k) * (1f - k);
                currentLevel = Mathf.Lerp(m_StartLevel, m_MaxLevel, k);
            }

            ApplyLevel(Mathf.Sin(Time.time * 1.3f) * m_Ripple);
        }

        void ApplyLevel(float ripple)
        {
            if (m_WaterSurface == null)
                return;
            var p = m_WaterSurface.position;
            p.y = currentLevel + ripple;
            m_WaterSurface.position = p;
        }

        void OnStarted()
        {
            m_Rising = true;
        }

        void OnComplete()
        {
            m_Frozen = true;
            if (m_ScenarioManager != null)
            {
                var cm = Mathf.Max(0, Mathf.RoundToInt(currentLevel * 100f));
                m_ScenarioManager.AddResultNote(cm > 0
                    ? $"A água estava com {cm} cm quando você chegou ao ponto de encontro."
                    : "Você concluiu antes de a água invadir a casa. Excelente!");
            }
        }
    }
}
