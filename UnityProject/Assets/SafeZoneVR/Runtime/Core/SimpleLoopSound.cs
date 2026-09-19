using UnityEngine;

namespace SafeZoneVR
{
    public class SimpleLoopSound : MonoBehaviour
    {
        public enum ClipKind { SmokeAlarm, DistantSiren, Crackle, Hail, Rain }

        [SerializeField] AudioSource m_Source;
        [SerializeField] ClipKind m_Kind = ClipKind.SmokeAlarm;
        [SerializeField] bool m_PlayOnStart;
        [SerializeField] bool m_ManageVolume = true;
        float m_BaseVolume = 1f;

        public AudioSource source { get => m_Source; set => m_Source = value; }
        public ClipKind kind { get => m_Kind; set => m_Kind = value; }
        public bool playOnStart { get => m_PlayOnStart; set => m_PlayOnStart = value; }
        public bool manageVolume { get => m_ManageVolume; set => m_ManageVolume = value; }

        public AudioCategory category => m_Kind == ClipKind.SmokeAlarm || m_Kind == ClipKind.Crackle ? AudioCategory.Effects : AudioCategory.Ambient;

        void Awake()
        {
            if (m_Source == null) m_Source = GetComponent<AudioSource>();
            if (m_Source == null) return;
            m_BaseVolume = m_Source.volume;
            m_Source.loop = true;
            m_Source.playOnAwake = false;
            if (m_Source.clip == null)
            {
                switch (m_Kind)
                {
                    case ClipKind.SmokeAlarm: m_Source.clip = ProceduralAudio.CreateSmokeAlarmLoop("sz_alarm"); break;
                    case ClipKind.DistantSiren: m_Source.clip = ProceduralAudio.CreateDistantSirenLoop("sz_siren"); break;
                    case ClipKind.Crackle: m_Source.clip = ProceduralAudio.CreateCrackleLoop("sz_crackle"); break;
                    case ClipKind.Hail: m_Source.clip = ProceduralAudio.CreateHailLoop("sz_hail"); break;
                    case ClipKind.Rain: m_Source.clip = ProceduralAudio.CreateRainLoop("sz_rain2"); break;
                }
            }
        }

        void OnEnable() { AudioVolumes.changed += ApplyVolume; ApplyVolume(); }
        void OnDisable() => AudioVolumes.changed -= ApplyVolume;

        void Start() { if (m_PlayOnStart) Play(); }

        void ApplyVolume()
        {
            if (m_ManageVolume && m_Source != null)
                m_Source.volume = m_BaseVolume * AudioVolumes.Get(category);
        }

        public void Play() { if (m_Source != null && !m_Source.isPlaying) m_Source.Play(); }
        public void Stop() { if (m_Source != null) m_Source.Stop(); }
        public void StopAfter(float seconds) { CancelInvoke(nameof(Stop)); Invoke(nameof(Stop), seconds); }
    }
}
