using UnityEngine;

namespace SafeZoneVR
{
    [RequireComponent(typeof(Collider))]
    public class StepCompleteOnCompanionInZone : StepValidatorBase
    {
        [SerializeField] CompanionFollower m_Companion;
        [SerializeField] float m_CheckInterval = 0.25f;
        Collider m_Zone; float m_NextCheck;

        public CompanionFollower companion { get => m_Companion; set => m_Companion = value; }

        protected override void Awake() { base.Awake(); m_Zone = GetComponent<Collider>(); m_Zone.isTrigger = true; }

        void Update()
        {
            if (m_Completed || Time.time < m_NextCheck || m_Companion == null) return;
            m_NextCheck = Time.time + m_CheckInterval;
            var m = manager;
            if (m == null || !m.hasStarted) return;
            var playerIn = PlayerLocator.TryGetHeadPosition(out var head) && m_Zone.bounds.Contains(head);
            var companionIn = m_Zone.bounds.Contains(m_Companion.transform.position + Vector3.up * 0.5f);
            if (playerIn && companionIn) Complete();
        }
    }
}
