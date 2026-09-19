using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SafeZoneVR
{
    public class NeighborIntruder : MonoBehaviour
    {
        [SerializeField] CompanionFollower m_Follower;
        [SerializeField] XRSimpleInteractable m_Interactable;
        [SerializeField] List<Transform> m_ToGate = new List<Transform>();
        [SerializeField] List<Transform> m_ToMeeting = new List<Transform>();
        [SerializeField] TextMeshPro m_Speech;
        [SerializeField] string m_ArrivingText = "Vou lá dentro pegar sua TV!";
        [SerializeField] string m_StoppedText = "Tem razão. Vamos esperar os Bombeiros.";
        bool m_Stopped;

        public CompanionFollower follower { get => m_Follower; set => m_Follower = value; }
        public XRSimpleInteractable interactable { get => m_Interactable; set => m_Interactable = value; }
        public List<Transform> toGate => m_ToGate;
        public List<Transform> toMeeting => m_ToMeeting;
        public TextMeshPro speech { get => m_Speech; set => m_Speech = value; }
        public bool stopped => m_Stopped;

        void OnEnable()
        {
            if (m_Interactable != null) m_Interactable.selectEntered.AddListener(OnStopped);
            if (m_Follower != null && !m_Stopped) m_Follower.Restart(m_ToGate);
            if (m_Speech != null && !m_Stopped) { m_Speech.text = m_ArrivingText; m_Speech.gameObject.SetActive(true); }
        }

        void OnDisable() { if (m_Interactable != null) m_Interactable.selectEntered.RemoveListener(OnStopped); }

        void OnStopped(SelectEnterEventArgs _) => Stop();

        public void Stop()
        {
            if (m_Stopped) return;
            m_Stopped = true;
            if (m_Follower != null) m_Follower.Restart(m_ToMeeting);
            if (m_Speech != null) m_Speech.text = m_StoppedText;
        }
    }
}
