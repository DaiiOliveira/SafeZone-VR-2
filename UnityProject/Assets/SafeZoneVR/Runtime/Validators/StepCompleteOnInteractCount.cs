using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SafeZoneVR
{
    public class StepCompleteOnInteractCount : StepValidatorBase
    {
        [SerializeField] List<XRBaseInteractable> m_Interactables = new List<XRBaseInteractable>();
        [SerializeField, Min(1)] int m_RequiredCount = 1;
        readonly List<XRBaseInteractable> m_Done = new List<XRBaseInteractable>();

        public List<XRBaseInteractable> interactables => m_Interactables;
        public int requiredCount { get => m_RequiredCount; set => m_RequiredCount = value; }

        void OnEnable()  { foreach (var i in m_Interactables) if (i != null) i.selectEntered.AddListener(OnSelect); }
        void OnDisable() { foreach (var i in m_Interactables) if (i != null) i.selectEntered.RemoveListener(OnSelect); }

        void OnSelect(SelectEnterEventArgs args)
        {
            if (m_Completed || args.interactorObject is XRSocketInteractor) return;
            var it = args.interactableObject as XRBaseInteractable;
            if (it == null || m_Done.Contains(it)) return;
            m_Done.Add(it);
            if (it.TryGetComponent<ObjectiveTarget>(out var target)) target.SetSatisfied(true);
            ReportProgress(m_Done.Count, m_RequiredCount);
            if (m_Done.Count >= m_RequiredCount) Complete();
        }
    }
}
