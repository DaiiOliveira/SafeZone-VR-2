using UnityEngine;

namespace SafeZoneVR
{
    [RequireComponent(typeof(Collider))]
    public class StepCompleteOnStayInZone : StepValidatorBase
    {
        [SerializeField] float m_RequiredSeconds = 30f;
        [SerializeField] float m_CheckInterval = 0.2f;
        [Tooltip("Só conta quando este passo é o atual (evita adiantar a espera).")]
        [SerializeField] bool m_OnlyWhenCurrent = true;
        Collider m_Zone; float m_Accumulated, m_NextCheck; int m_LastReported = -1;

        public float requiredSeconds { get => m_RequiredSeconds; set => m_RequiredSeconds = value; }
        public bool onlyWhenCurrent { get => m_OnlyWhenCurrent; set => m_OnlyWhenCurrent = value; }

        protected override void Awake() { base.Awake(); m_Zone = GetComponent<Collider>(); m_Zone.isTrigger = true; }

        void Update()
        {
            if (m_Completed || Time.time < m_NextCheck) return;
            m_NextCheck = Time.time + m_CheckInterval;
            var m = manager;
            if (m == null || !m.hasStarted || (m_OnlyWhenCurrent && !m.IsCurrentStep(m_Step))) return;
            if (!PlayerLocator.TryGetHeadPosition(out var head) || !m_Zone.bounds.Contains(head)) return;

            m_Accumulated += m_CheckInterval;
            var shown = Mathf.FloorToInt(m_Accumulated / 5f) * 5;
            if (shown != m_LastReported) { m_LastReported = shown; ReportProgress(shown, Mathf.RoundToInt(m_RequiredSeconds)); }
            if (m_Accumulated >= m_RequiredSeconds) Complete();
        }
    }
}
