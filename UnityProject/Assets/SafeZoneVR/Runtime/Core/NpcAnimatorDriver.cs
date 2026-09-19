using TMPro;
using UnityEngine;

namespace SafeZoneVR
{
    public class NpcAnimatorDriver : MonoBehaviour
    {
        static readonly int k_Walking = Animator.StringToHash("Andando");
        static readonly int k_Point = Animator.StringToHash("Apontar");

        [SerializeField] Animator m_Animator;
        [SerializeField] TextMeshPro m_SpeechBubble;
        [SerializeField] bool m_PointWhenSpeaking = true;
        [SerializeField] float m_WalkSpeedThreshold = 0.08f;
        [SerializeField] float m_ReferenceWalkSpeed = 0.9f;

        Vector3 m_LastPosition;
        float m_SmoothedSpeed;
        bool m_WasSpeaking;
        string m_LastSpeech;

        public Animator animator { get => m_Animator; set => m_Animator = value; }
        public TextMeshPro speechBubble { get => m_SpeechBubble; set => m_SpeechBubble = value; }
        public float referenceWalkSpeed { get => m_ReferenceWalkSpeed; set => m_ReferenceWalkSpeed = value; }
        public bool pointWhenSpeaking { get => m_PointWhenSpeaking; set => m_PointWhenSpeaking = value; }

        void Awake()
        {
            if (m_Animator == null) m_Animator = GetComponentInChildren<Animator>();
            m_LastPosition = transform.position;
        }

        void Update()
        {
            if (m_Animator == null) return;

            var dt = Mathf.Max(Time.deltaTime, 0.0001f);
            var delta = transform.position - m_LastPosition;
            delta.y = 0f;
            m_LastPosition = transform.position;
            m_SmoothedSpeed = Mathf.Lerp(m_SmoothedSpeed, delta.magnitude / dt, 10f * dt);

            var walking = m_SmoothedSpeed > m_WalkSpeedThreshold;
            m_Animator.SetBool(k_Walking, walking);
            m_Animator.speed = walking && m_ReferenceWalkSpeed > 0f ? Mathf.Clamp(m_SmoothedSpeed / m_ReferenceWalkSpeed, 0.6f, 1.6f) : 1f;

            if (!m_PointWhenSpeaking || m_SpeechBubble == null) return;
            var speaking = m_SpeechBubble.gameObject.activeInHierarchy && !string.IsNullOrEmpty(m_SpeechBubble.text);
            if (speaking && (!m_WasSpeaking || m_SpeechBubble.text != m_LastSpeech) && !walking)
                Point();
            m_WasSpeaking = speaking;
            m_LastSpeech = speaking ? m_SpeechBubble.text : null;
        }

        public void Point()
        {
            if (m_Animator != null)
                m_Animator.SetTrigger(k_Point);
        }
    }
}
