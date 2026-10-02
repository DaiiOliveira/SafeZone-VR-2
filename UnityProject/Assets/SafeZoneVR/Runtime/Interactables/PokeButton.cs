using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SafeZoneVR
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class PokeButton : MonoBehaviour
    {
        [SerializeField] Transform m_Cap;
        [SerializeField] GameObject m_PressedVisual;
        [SerializeField] float m_PressDepth = 0.008f;
        [SerializeField] float m_Cooldown = 0.4f;
        [SerializeField] UnityEvent m_OnPressed = new UnityEvent();

        static AudioClip s_Click;
        XRSimpleInteractable m_Interactable;
        AudioSource m_Audio;
        Vector3 m_CapRest;
        float m_LastPress = -10f;

        public Transform cap { get => m_Cap; set => m_Cap = value; }
        public GameObject pressedVisual { get => m_PressedVisual; set => m_PressedVisual = value; }
        public UnityEvent onPressed => m_OnPressed;

        void Awake()
        {
            if (m_Cap != null) m_CapRest = m_Cap.localPosition;
            if (m_PressedVisual != null) m_PressedVisual.SetActive(false);
            if (s_Click == null) s_Click = ProceduralAudio.CreateChime("sz_poke", new[] { 880f }, 0.05f, 0.4f);
            m_Audio = gameObject.AddComponent<AudioSource>();
            m_Audio.playOnAwake = false;
            m_Audio.spatialBlend = 0f;
            m_Interactable = GetComponent<XRSimpleInteractable>();
            m_Interactable.selectEntered.AddListener(OnSelectEntered);
            m_Interactable.selectExited.AddListener(OnSelectExited);
        }

        void OnDestroy()
        {
            if (m_Interactable == null) return;
            m_Interactable.selectEntered.RemoveListener(OnSelectEntered);
            m_Interactable.selectExited.RemoveListener(OnSelectExited);
        }

        void OnSelectEntered(SelectEnterEventArgs args)
        {
            if (m_Cap != null) m_Cap.localPosition = m_CapRest - Vector3.up * m_PressDepth;
            if (m_PressedVisual != null) m_PressedVisual.SetActive(true);
            if (Time.time - m_LastPress < m_Cooldown) return;
            m_LastPress = Time.time;
            m_Audio.PlayOneShot(s_Click, AudioVolumes.Get(AudioCategory.Effects));
            m_OnPressed.Invoke();
        }

        void OnSelectExited(SelectExitEventArgs args)
        {
            if (m_Cap != null) m_Cap.localPosition = m_CapRest;
            if (m_PressedVisual != null) m_PressedVisual.SetActive(false);
        }
    }
}
