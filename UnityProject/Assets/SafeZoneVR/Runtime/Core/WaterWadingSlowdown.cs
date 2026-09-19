using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

namespace SafeZoneVR
{
    public class WaterWadingSlowdown : MonoBehaviour
    {
        [SerializeField] FloodWaterController m_Water;
        [SerializeField] float m_DepthForMinimum = 0.4f;
        [SerializeField, Range(0.1f, 1f)] float m_MinimumFactor = 0.4f;
        [SerializeField] float m_NoticeDepth = 0.08f;
        [SerializeField, TextArea] string m_Notice = "A água dificulta andar: você está mais lento. Na vida real, a correnteza pode derrubar e esconder buracos.";
        [SerializeField] float m_CheckInterval = 0.2f;
        [SerializeField] float m_ProbeHeight = 1.5f;

        ContinuousMoveProvider m_Move;
        float m_BaseMoveSpeed;
        readonly List<XRRayInteractor> m_TeleportRays = new List<XRRayInteractor>();
        readonly List<float> m_BaseVelocities = new List<float>();
        float m_NextCheck;
        readonly RaycastHit[] m_Hits = new RaycastHit[8];
        bool m_Noticed;

        public FloodWaterController water { get => m_Water; set => m_Water = value; }
        public float currentDepth { get; private set; }
        public float currentFactor { get; private set; } = 1f;

        void Start()
        {
            if (m_Water == null)
                m_Water = FindAnyObjectByType<FloodWaterController>();

            var origin = PlayerLocator.origin;
            if (origin == null) return;

            m_Move = origin.GetComponentInChildren<ContinuousMoveProvider>(true);
            if (m_Move != null)
                m_BaseMoveSpeed = m_Move.moveSpeed;

            foreach (var ray in origin.GetComponentsInChildren<XRRayInteractor>(true))
            {
                if (!ray.name.Contains("Teleport")) continue;
                m_TeleportRays.Add(ray);
                m_BaseVelocities.Add(ray.velocity);
            }
        }

        void OnDisable() => ApplyFactor(1f);

        void Update()
        {
            if (Time.time < m_NextCheck || m_Water == null) return;
            m_NextCheck = Time.time + m_CheckInterval;

            currentDepth = MeasureDepth();
            var k = m_DepthForMinimum <= 0f ? 1f : Mathf.Clamp01(currentDepth / m_DepthForMinimum);
            ApplyFactor(Mathf.Lerp(1f, m_MinimumFactor, k));

            if (!m_Noticed && currentDepth >= m_NoticeDepth && ScenarioManager.instance != null && ScenarioManager.instance.hasStarted)
            {
                m_Noticed = true;
                ScenarioManager.instance.ShowInfo(m_Notice);
            }
        }

        float MeasureDepth()
        {
            if (!PlayerLocator.TryGetFeetPosition(out var feet) || !PlayerLocator.TryGetHeadPosition(out var head))
                return 0f;
            var from = new Vector3(head.x, feet.y + m_ProbeHeight, head.z);
            var floorY = feet.y;
            var count = Physics.RaycastNonAlloc(from, Vector3.down, m_Hits, m_ProbeHeight + 1f, ~0, QueryTriggerInteraction.Ignore);
            var best = float.MaxValue;
            for (var i = 0; i < count; i++)
            {
                var h = m_Hits[i];
                if (h.rigidbody != null || PlayerLocator.IsPlayerCollider(h.collider)) continue;
                if (h.distance < best) { best = h.distance; floorY = h.point.y; }
            }
            return Mathf.Max(0f, m_Water.currentLevel - floorY);
        }

        void ApplyFactor(float factor)
        {
            if (Mathf.Approximately(factor, currentFactor)) return;
            currentFactor = factor;
            if (m_Move != null)
                m_Move.moveSpeed = m_BaseMoveSpeed * factor;
            for (var i = 0; i < m_TeleportRays.Count; i++)
                if (m_TeleportRays[i] != null)
                    m_TeleportRays[i].velocity = m_BaseVelocities[i] * Mathf.Lerp(1f, factor, 0.8f);
        }
    }
}
