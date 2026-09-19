using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SafeZoneVR
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class DecisionOption : MonoBehaviour
    {
        [SerializeField] bool m_IsCorrect;
        [SerializeField] string m_MistakeId;
        [SerializeField, TextArea] string m_Feedback;
        [SerializeField, TextArea] string m_Advice;

        public bool isCorrect { get => m_IsCorrect; set => m_IsCorrect = value; }
        public string mistakeId { get => m_MistakeId; set => m_MistakeId = value; }
        public string feedback { get => m_Feedback; set => m_Feedback = value; }
        public string advice { get => m_Advice; set => m_Advice = value; }
        public event Action<DecisionOption> chosen;

        void Awake() => GetComponent<XRSimpleInteractable>().selectEntered.AddListener(_ => Choose());

        public void Choose() => chosen?.Invoke(this);
    }
}
