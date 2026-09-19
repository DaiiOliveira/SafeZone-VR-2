using UnityEngine;
using UnityEngine.Events;

namespace SafeZoneVR
{
    public class PlayerIntegrity : MonoBehaviour
    {
        [SerializeField] float m_Max = 100f;
        [SerializeField] float m_RecoverPerSecond = 4f;
        [SerializeField] float m_RecoverDelaySeconds = 6f;
        [SerializeField] FeedbackPlayer m_Feedback;
        [SerializeField, TextArea] string m_DepletedAdvice = "Saia da área de risco assim que perceber o perigo: fumaça, calor ou impacto vão te derrubar se você ficar.";
        [SerializeField] UnityEvent m_OnDepleted = new UnityEvent();

        float m_Value;
        float m_LastDamage = -99f;

        static PlayerIntegrity s_Instance;

        public static PlayerIntegrity instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = FindAnyObjectByType<PlayerIntegrity>();
                return s_Instance;
            }
        }

        public float value => m_Value;
        public float max => m_Max;
        public float normalized => m_Max > 0f ? Mathf.Clamp01(m_Value / m_Max) : 0f;
        public bool isHurt => m_Value < m_Max - 0.5f;
        public FeedbackPlayer feedback { get => m_Feedback; set => m_Feedback = value; }
        public UnityEvent onDepleted => m_OnDepleted;

        void Awake()
        {
            s_Instance = this;
            m_Value = m_Max;
        }

        void OnDestroy()
        {
            if (s_Instance == this)
                s_Instance = null;
        }

        void Update()
        {
            if (m_Value >= m_Max || m_RecoverPerSecond <= 0f) return;
            if (Time.time - m_LastDamage < m_RecoverDelaySeconds) return;
            var manager = ScenarioManager.instance;
            if (manager != null && manager.isComplete) return;
            m_Value = Mathf.Min(m_Max, m_Value + m_RecoverPerSecond * Time.deltaTime);
        }

        public void Damage(float amount, string reason)
        {
            if (amount <= 0f || m_Value <= 0f) return;
            var manager = ScenarioManager.instance;
            if (manager != null && (manager.isComplete || !manager.hasStarted)) return;

            var before = m_Value;
            m_Value = Mathf.Max(0f, m_Value - amount);
            m_LastDamage = Time.time;

            if (m_Feedback != null && Mathf.FloorToInt(before / 10f) != Mathf.FloorToInt(m_Value / 10f))
                m_Feedback.PlayHurt();

            if (m_Value > 0f)
                return;

            m_OnDepleted.Invoke();
            if (manager == null)
                return;
            var message = string.IsNullOrEmpty(reason) ? "Você não aguentou as condições do ambiente." : reason;
            manager.RegisterMistake("integridade_zerada", message, 0f, ActionKind.ErroGrave, true, m_DepletedAdvice);
        }

        public void Reset()
        {
            m_Value = m_Max;
            m_LastDamage = -99f;
        }
    }
}
