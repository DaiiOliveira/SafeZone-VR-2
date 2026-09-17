using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SafeZoneVR
{
    public class FireController : MonoBehaviour
    {
        public enum Size { Out = 0, Small = 1, Medium = 2, Large = 3 }

        [SerializeField] ParticleSystem m_Flames;
        [SerializeField] ParticleSystem m_Smoke;
        [SerializeField] ParticleSystem m_Sparks;
        [SerializeField] Light m_Light;
        [SerializeField] AudioSource m_Crackle;
        [SerializeField] float[] m_FlameRate = { 0f, 12f, 30f, 60f };
        [SerializeField] float[] m_FlameScale = { 0f, 0.35f, 0.8f, 1.4f };
        [SerializeField] float[] m_LightIntensity = { 0f, 0.8f, 1.6f, 2.6f };
        [SerializeField] Size m_StartSize = Size.Small;
        [SerializeField] float[] m_GrowSteps = new float[0];
        [SerializeField] float m_ResidualSmokeSeconds = 20f;
        Size m_Size; int m_GrowIndex; float m_NextGrow = -1f, m_NextFlicker; bool m_Flaring;

        public Size size => m_Size;
        public bool isBurning => m_Size != Size.Out;
        public ParticleSystem flames { get => m_Flames; set => m_Flames = value; }
        public ParticleSystem smoke { get => m_Smoke; set => m_Smoke = value; }
        public ParticleSystem sparks { get => m_Sparks; set => m_Sparks = value; }
        public Light fireLight { get => m_Light; set => m_Light = value; }
        public AudioSource crackle { get => m_Crackle; set => m_Crackle = value; }
        public Size startSize { get => m_StartSize; set => m_StartSize = value; }
        public float[] growSteps { get => m_GrowSteps; set => m_GrowSteps = value; }
        public float residualSmokeSeconds { get => m_ResidualSmokeSeconds; set => m_ResidualSmokeSeconds = value; }

        void Awake() => SetSize(m_StartSize);
        void OnEnable() => AudioVolumes.changed += RefreshVolume;
        void OnDisable() => AudioVolumes.changed -= RefreshVolume;
        void RefreshVolume() { if (m_Crackle != null) m_Crackle.volume = 0.15f * (int)m_Size * AudioVolumes.Get(AudioCategory.Effects); }

        void Start() { if (isBurning) ScheduleGrowth(); }

        public void Ignite(Size s) { SetSize(s == Size.Out ? Size.Small : s); ScheduleGrowth(); }

        public void IgniteSmall() => Ignite(Size.Small);

        public void SparkThenIgnite() => StartCoroutine(SparkRoutine());

        public void Extinguish()
        {
            SetSize(Size.Out);
            m_NextGrow = -1f;
            if (m_Smoke != null) { m_Smoke.Play(); CancelInvoke(nameof(StopSmoke)); Invoke(nameof(StopSmoke), m_ResidualSmokeSeconds); }
        }

        public void Flare()
        {
            if (!isBurning || m_Flaring) return;
            StartCoroutine(FlareRoutine());
        }

        IEnumerator SparkRoutine()
        {
            if (m_Sparks != null) m_Sparks.Play();
            yield return new WaitForSeconds(2f);
            if (m_Sparks != null) m_Sparks.Stop();
            Ignite(Size.Small);
        }

        IEnumerator FlareRoutine()
        {
            m_Flaring = true;
            var before = m_Size;
            SetSize((Size)Mathf.Min((int)Size.Large, (int)before + 2));
            yield return new WaitForSeconds(0.6f);
            SetSize(before);
            m_Flaring = false;
        }

        void ScheduleGrowth()
        {
            m_NextGrow = m_GrowIndex < m_GrowSteps.Length ? Time.time + m_GrowSteps[m_GrowIndex] : -1f;
        }

        void Update()
        {
            if (!isBurning) return;
            if (m_NextGrow > 0f && Time.time >= m_NextGrow && m_Size < Size.Large)
            {
                SetSize(m_Size + 1);
                m_GrowIndex++;
                ScheduleGrowth();
            }
            if (m_Light != null && Time.time >= m_NextFlicker)
            {
                m_NextFlicker = Time.time + 0.05f;
                m_Light.intensity = m_LightIntensity[(int)m_Size] * (0.85f + 0.3f * Mathf.PerlinNoise(Time.time * 6f, 0f));
            }
        }

        void SetSize(Size s)
        {
            m_Size = s;
            var i = (int)s;
            if (m_Flames != null)
            {
                var e = m_Flames.emission; e.rateOverTime = m_FlameRate[i];
                m_Flames.transform.localScale = Vector3.one * Mathf.Max(0.01f, m_FlameScale[i]);
                if (i > 0 && !m_Flames.isPlaying) m_Flames.Play();
                if (i == 0) m_Flames.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            if (m_Smoke != null && i > 0 && !m_Smoke.isPlaying) m_Smoke.Play();
            if (m_Light != null) { m_Light.enabled = i > 0; m_Light.intensity = m_LightIntensity[i]; }
            if (m_Crackle != null)
            {
                m_Crackle.volume = 0.15f * i * AudioVolumes.Get(AudioCategory.Effects);
                if (i > 0 && !m_Crackle.isPlaying) m_Crackle.Play();
                if (i == 0) m_Crackle.Stop();
            }
        }

        void StopSmoke() { if (m_Smoke != null) m_Smoke.Stop(true, ParticleSystemStopBehavior.StopEmitting); }
    }
}
