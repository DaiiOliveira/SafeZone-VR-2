using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

namespace SafeZoneVR
{
    public class WallClipGuard : MonoBehaviour
    {
        [SerializeField] float m_FullFadeDepth = 0.18f;
        [SerializeField] float m_MaxAlpha = 0.92f;
        [SerializeField] float m_FadeSpeed = 6f;
        [SerializeField] float m_CheckInterval = 0.05f;
        [SerializeField] float m_TeleportJump = 1.2f;
        [SerializeField] float m_HapticAmplitude = 0.35f;
        [SerializeField] float m_HapticDuration = 0.12f;

        readonly RaycastHit[] m_Hits = new RaycastHit[8];
        readonly List<HapticImpulsePlayer> m_Haptics = new List<HapticImpulsePlayer>();
        float m_NextCheck;
        float m_Target;
        float m_Alpha;
        bool m_WasBlocked;
        bool m_HasSafe;
        Vector3 m_SafeHead;

        public bool isBlocked => m_Target > 0.01f;
        public float overlayAlpha => m_Alpha;

        void Start()
        {
            var origin = PlayerLocator.origin;
            if (origin != null)
                m_Haptics.AddRange(origin.GetComponentsInChildren<HapticImpulsePlayer>(true));
        }

        void OnDisable()
        {
            m_Alpha = 0f;
            m_Target = 0f;
            m_HasSafe = false;
            var fader = ScreenFader.instance;
            if (fader != null && !fader.isBusy)
                fader.SetOverlay(0f);
        }

        void Update()
        {
            if (Time.time >= m_NextCheck)
            {
                m_NextCheck = Time.time + m_CheckInterval;
                m_Target = Measure();

                var blocked = m_Target > 0.01f;
                if (blocked && !m_WasBlocked)
                    Pulse();
                m_WasBlocked = blocked;
            }

            var next = Mathf.MoveTowards(m_Alpha, m_Target, m_FadeSpeed * Time.deltaTime);
            if (Mathf.Approximately(next, m_Alpha))
                return;
            m_Alpha = next;

            var fader = ScreenFader.instance;
            if (fader != null && !fader.isBusy)
                fader.SetOverlay(m_Alpha);
        }

        float Measure()
        {
            if (!PlayerLocator.TryGetHeadPosition(out var head))
                return 0f;

            if (!m_HasSafe)
            {
                m_SafeHead = head;
                m_HasSafe = true;
                return 0f;
            }

            var delta = head - m_SafeHead;
            var distance = delta.magnitude;
            if (distance > m_TeleportJump)
            {
                m_SafeHead = head;
                return 0f;
            }

            if (distance > 0.0005f)
            {
                var count = Physics.RaycastNonAlloc(m_SafeHead, delta / distance, m_Hits, distance, ~0, QueryTriggerInteraction.Ignore);
                var nearest = -1f;
                for (var i = 0; i < count; i++)
                {
                    var h = m_Hits[i];
                    if (h.collider == null || h.collider.attachedRigidbody != null || PlayerLocator.IsPlayerCollider(h.collider)) continue;
                    if (nearest < 0f || h.distance < nearest) nearest = h.distance;
                }

                if (nearest >= 0f)
                {
                    var depth = distance - nearest;
                    return Mathf.Clamp01(depth / m_FullFadeDepth) * m_MaxAlpha;
                }
            }

            m_SafeHead = head;
            return 0f;
        }

        void Pulse()
        {
            for (var i = 0; i < m_Haptics.Count; i++)
                if (m_Haptics[i] != null && m_Haptics[i].isActiveAndEnabled)
                    m_Haptics[i].SendHapticImpulse(m_HapticAmplitude, m_HapticDuration);
        }
    }
}
