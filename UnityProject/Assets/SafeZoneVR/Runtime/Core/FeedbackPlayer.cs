using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

namespace SafeZoneVR
{
    [RequireComponent(typeof(AudioSource))]
    public class FeedbackPlayer : MonoBehaviour
    {
        [SerializeField]
        ScenarioManager m_ScenarioManager;

        [SerializeField]
        bool m_PlayRainAmbience = true;

        [SerializeField]
        [Range(0f, 1f)]
        float m_RainVolume = 0.18f;

        AudioSource m_Sfx;
        AudioSource m_Ambience;
        bool m_Faded;
        AudioClip m_StepClip;
        AudioClip m_ProgressClip;
        AudioClip m_MistakeClip;
        AudioClip m_CompleteClip;
        readonly List<HapticImpulsePlayer> m_Haptics = new List<HapticImpulsePlayer>();

        public ScenarioManager scenarioManager { get => m_ScenarioManager; set => m_ScenarioManager = value; }
        public bool playRainAmbience { get => m_PlayRainAmbience; set => m_PlayRainAmbience = value; }

        void Awake()
        {
            m_Sfx = GetComponent<AudioSource>();
            m_Sfx.playOnAwake = false;
            m_Sfx.spatialBlend = 0f;

            m_StepClip = ProceduralAudio.CreateChime("sz_step", new[] { 523.25f, 659.25f, 783.99f });
            m_ProgressClip = ProceduralAudio.CreateChime("sz_progress", new[] { 659.25f }, 0.12f, 0.35f);
            m_MistakeClip = ProceduralAudio.CreateSoftWarning("sz_warn");
            m_CompleteClip = ProceduralAudio.CreateChime("sz_complete", new[] { 523.25f, 659.25f, 783.99f, 1046.5f }, 0.18f, 0.55f);

            if (m_PlayRainAmbience)
            {
                var amb = new GameObject("RainAmbience");
                amb.transform.SetParent(transform, false);
                m_Ambience = amb.AddComponent<AudioSource>();
                m_Ambience.clip = ProceduralAudio.CreateRainLoop("sz_rain");
                m_Ambience.loop = true;
                m_Ambience.spatialBlend = 0f;
                m_Ambience.volume = m_RainVolume * AudioVolumes.Get(AudioCategory.Ambient);
                m_Ambience.playOnAwake = false;
            }
        }

        void Start()
        {
            if (m_ScenarioManager == null)
                m_ScenarioManager = ScenarioManager.instance;

            var origin = PlayerLocator.origin;
            if (origin != null)
                m_Haptics.AddRange(origin.GetComponentsInChildren<HapticImpulsePlayer>(true));

            if (m_ScenarioManager != null)
            {
                m_ScenarioManager.onStepCompleted.AddListener(OnStepCompleted);
                m_ScenarioManager.onStepProgress.AddListener(OnStepProgress);
                m_ScenarioManager.onMistake.AddListener(OnMistake);
                m_ScenarioManager.onScenarioComplete.AddListener(OnScenarioComplete);
            }

            if (m_Ambience != null)
                m_Ambience.Play();
        }

        void OnEnable() => AudioVolumes.changed += OnVolumesChanged;
        void OnDisable() => AudioVolumes.changed -= OnVolumesChanged;

        void OnVolumesChanged()
        {
            if (m_Ambience != null && !m_Faded)
                m_Ambience.volume = m_RainVolume * AudioVolumes.Get(AudioCategory.Ambient);
        }

        public void PlayHurt()
        {
            PlaySfx(m_MistakeClip, 0.9f);
            Pulse(0.7f, 0.35f);
        }

        public void PlayHint()
        {
            PlaySfx(m_ProgressClip, 0.7f);
            Pulse(0.25f, 0.08f);
        }

        void OnDestroy()
        {
            if (m_ScenarioManager != null)
            {
                m_ScenarioManager.onStepCompleted.RemoveListener(OnStepCompleted);
                m_ScenarioManager.onStepProgress.RemoveListener(OnStepProgress);
                m_ScenarioManager.onMistake.RemoveListener(OnMistake);
                m_ScenarioManager.onScenarioComplete.RemoveListener(OnScenarioComplete);
            }
        }

        public void PlayUiClick()
        {
            PlaySfx(m_ProgressClip, 0.5f);
        }

        public void SetRainVolume(float volume)
        {
            m_RainVolume = Mathf.Clamp01(volume);
            if (m_Ambience != null)
            {
                m_Ambience.volume = m_RainVolume * AudioVolumes.Get(AudioCategory.Ambient);
                if (m_RainVolume > 0f && !m_Ambience.isPlaying)
                    m_Ambience.Play();
            }
        }

        void OnStepCompleted(MissionStepSO step)
        {
            PlaySfx(m_StepClip);
            Pulse(0.6f, 0.15f);
        }

        void OnStepProgress(MissionStepSO step, int done, int total)
        {
            if (done < total)
            {
                PlaySfx(m_ProgressClip);
                Pulse(0.3f, 0.06f);
            }
        }

        void OnMistake(string message)
        {
            PlaySfx(m_MistakeClip);
            Pulse(0.4f, 0.25f);
        }

        void OnScenarioComplete()
        {
            var failed = m_ScenarioManager != null && m_ScenarioManager.failed;
            PlaySfx(failed ? m_MistakeClip : m_CompleteClip);
            Pulse(failed ? 0.9f : 0.8f, failed ? 0.45f : 0.3f);
            if (m_Ambience != null)
                StartCoroutine(FadeOutAmbience());
        }

        IEnumerator FadeOutAmbience()
        {
            m_Faded = true;
            var start = m_Ambience.volume;
            var t = 0f;
            while (t < 3f)
            {
                t += Time.deltaTime;
                m_Ambience.volume = Mathf.Lerp(start, start * 0.35f, t / 3f);
                yield return null;
            }
        }

        void PlaySfx(AudioClip clip, float volume = 1f)
        {
            if (clip != null && m_Sfx != null && m_Sfx.isActiveAndEnabled)
                m_Sfx.PlayOneShot(clip, volume * AudioVolumes.Get(AudioCategory.Effects));
        }

        void Pulse(float amplitude, float duration)
        {
            for (var i = 0; i < m_Haptics.Count; i++)
            {
                if (m_Haptics[i] != null && m_Haptics[i].isActiveAndEnabled)
                    m_Haptics[i].SendHapticImpulse(amplitude, duration);
            }
        }
    }
}
