using System.Collections.Generic;
using UnityEngine;

namespace SafeZoneVR
{
    public class HailController : MonoBehaviour
    {
        static readonly int k_BaseColor = Shader.PropertyToID("_BaseColor");

        [SerializeField] ParticleSystem m_Hail;
        [SerializeField] float m_MaxEmission = 260f;
        [SerializeField] float m_EmitterHeight = 6f;
        [SerializeField] List<Renderer> m_IceLayers = new List<Renderer>();
        [SerializeField] float m_AccumulateSeconds = 40f;
        [SerializeField] float m_MaxIceAlpha = 0.6f;
        [SerializeField] List<AudioSource> m_RoofSounds = new List<AudioSource>();
        [SerializeField] MissionStepSO m_StopStep;
        [SerializeField] float m_StopWhenSecondsLeft = 8f;
        [SerializeField] float m_UpdateInterval = 0.1f;
        float m_Intensity, m_Target, m_Rate, m_Ice, m_NextUpdate;
        MaterialPropertyBlock m_Block; Transform m_Head;

        public float intensity => m_Intensity;
        public float ice => m_Ice;
        public ParticleSystem hail { get => m_Hail; set => m_Hail = value; }
        public List<Renderer> iceLayers => m_IceLayers;
        public List<AudioSource> roofSounds => m_RoofSounds;
        public MissionStepSO stopStep { get => m_StopStep; set => m_StopStep = value; }

        void Awake() { m_Block = new MaterialPropertyBlock(); Apply(); }

        void Start()
        {
            var m = ScenarioManager.instance;
            if (m != null) { m.onStepProgress.AddListener(OnProgress); m.onStepCompleted.AddListener(OnStepCompleted); }
            var cam = PlayerLocator.playerCamera;
            if (cam != null) m_Head = cam.transform;
        }

        void OnDestroy()
        {
            var m = ScenarioManager.instance;
            if (m != null) { m.onStepProgress.RemoveListener(OnProgress); m.onStepCompleted.RemoveListener(OnStepCompleted); }
        }

        void OnStepCompleted(MissionStepSO step)
        {
            if (step == m_StopStep && m_Target > 0f) RampTo(0f, 2f);
        }

        public void RampTo(float target, float seconds)
        {
            m_Target = Mathf.Clamp01(target);
            m_Rate = Mathf.Abs(m_Target - m_Intensity) / Mathf.Max(0.1f, seconds);
            if (m_Target > 0f && m_Hail != null && !m_Hail.isPlaying) m_Hail.Play();
            foreach (var s in m_RoofSounds) if (s != null && m_Target > 0f && !s.isPlaying) s.Play();
        }

        public void StartHail() { m_Intensity = Mathf.Max(m_Intensity, 0.3f); RampTo(1f, 15f); }

        void OnProgress(MissionStepSO step, int done, int total)
        {
            if (step == m_StopStep && total - done <= m_StopWhenSecondsLeft) RampTo(0f, m_StopWhenSecondsLeft);
        }

        void Update()
        {
            if (m_Head != null && m_Hail != null)
            {
                var p = m_Head.position;
                m_Hail.transform.position = new Vector3(p.x, p.y + m_EmitterHeight, p.z);
            }
            if (Time.time < m_NextUpdate) return;
            m_NextUpdate = Time.time + m_UpdateInterval;
            m_Intensity = Mathf.MoveTowards(m_Intensity, m_Target, m_Rate * m_UpdateInterval);
            if (m_Intensity > 0.2f) m_Ice = Mathf.Min(1f, m_Ice + m_UpdateInterval / m_AccumulateSeconds * m_Intensity);
            Apply();
        }

        void Apply()
        {
            if (m_Hail != null)
            {
                var e = m_Hail.emission;
                e.rateOverTime = m_MaxEmission * m_Intensity;
                if (m_Intensity <= 0.01f && m_Target <= 0f && m_Hail.isPlaying) m_Hail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            foreach (var r in m_IceLayers)
            {
                if (r == null) continue;
                m_Block.SetColor(k_BaseColor, new Color(0.92f, 0.95f, 1f, m_Ice * m_MaxIceAlpha));
                r.SetPropertyBlock(m_Block);
            }
            foreach (var s in m_RoofSounds)
            {
                if (s == null) continue;
                s.volume = 0.5f * m_Intensity * AudioVolumes.Get(AudioCategory.Ambient);
                if (m_Intensity <= 0.01f && m_Target <= 0f && s.isPlaying) s.Stop();
            }
        }
    }
}
