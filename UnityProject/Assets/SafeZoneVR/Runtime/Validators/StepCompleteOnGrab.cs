using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SafeZoneVR
{
    public class StepCompleteOnGrab : StepValidatorBase
    {
        [SerializeField]
        [Tooltip("Interagível a ser pego. Se vazio, usa o do próprio objeto.")]
        XRBaseInteractable m_Interactable;

        public XRBaseInteractable interactable
        {
            get => m_Interactable;
            set => m_Interactable = value;
        }

        protected override void Awake()
        {
            base.Awake();
            if (m_Interactable == null)
                m_Interactable = GetComponent<XRBaseInteractable>();
        }

        void OnEnable()
        {
            if (m_Interactable != null)
                m_Interactable.selectEntered.AddListener(OnSelectEntered);
        }

        void OnDisable()
        {
            if (m_Interactable != null)
                m_Interactable.selectEntered.RemoveListener(OnSelectEntered);
        }

        void OnSelectEntered(SelectEnterEventArgs args)
        {
            if (m_Completed)
                return;
            if (args.interactorObject is XRSocketInteractor)
                return;
            Complete();
        }

        public void OnGrabbed()
        {
            Complete();
        }
    }
}
