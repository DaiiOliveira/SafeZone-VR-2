using UnityEngine;

namespace SafeZoneVR
{
    /// <summary>Fonte de áudio simples (detector de fumaça, sirene) com métodos públicos para listeners persistentes.</summary>
    public class SimpleLoopSound : MonoBehaviour
    {
        public enum ClipKind { SmokeAlarm, DistantSiren, Crackle, Hail, Rain }

        [SerializeField] AudioSource m_Source;
        [SerializeField] ClipKind m_Kind = ClipKind.SmokeAlarm;
        [SerializeField] bool m_PlayOnStart;

        public AudioSource source { get => m_Source; set => m_Source = value; }
        public ClipKind kind { get => m_Kind; set => m_Kind = value; }
        public bool playOnStart { get => m_PlayOnStart; set => m_PlayOnStart = value; }

        void Awake()
        {
            if (m_Source == null) m_Source = GetComponent<AudioSource>();
            if (m_Source == null) return;
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

        void Start() { if (m_PlayOnStart) Play(); }

        public void Play() { if (m_Source != null && !m_Source.isPlaying) m_Source.Play(); }
        public void Stop() { if (m_Source != null) m_Source.Stop(); }
        public void StopAfter(float seconds) { CancelInvoke(nameof(Stop)); Invoke(nameof(Stop), seconds); }
    }
}
