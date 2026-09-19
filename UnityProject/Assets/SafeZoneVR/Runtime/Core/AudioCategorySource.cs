using UnityEngine;

namespace SafeZoneVR
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioCategorySource : MonoBehaviour
    {
        [SerializeField] AudioCategory m_Category = AudioCategory.Narration;
        [SerializeField, Range(0f, 1f)] float m_BaseVolume = 1f;

        AudioSource m_Source;

        public AudioCategory category { get => m_Category; set => m_Category = value; }
        public float baseVolume { get => m_BaseVolume; set { m_BaseVolume = value; Apply(); } }

        void Awake() => m_Source = GetComponent<AudioSource>();
        void OnEnable() { AudioVolumes.changed += Apply; Apply(); }
        void OnDisable() => AudioVolumes.changed -= Apply;

        void Apply()
        {
            if (m_Source != null)
                m_Source.volume = m_BaseVolume * AudioVolumes.Get(m_Category);
        }
    }
}
